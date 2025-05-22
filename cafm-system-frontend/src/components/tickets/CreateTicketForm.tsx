import React, { useState, useEffect } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { useNavigate } from 'react-router-dom';
import { Alert, Button, Card, Form as BootstrapForm, Badge } from 'react-bootstrap';
import ticketService, { KeywordSuggestion } from '../../api/ticketService';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTags, faLightbulb } from '@fortawesome/free-solid-svg-icons';

// Validation schema
const TicketSchema = Yup.object().shape({
  title: Yup.string()
    .required('Title is required')
    .min(5, 'Title must be at least 5 characters')
    .max(100, 'Title must be at most 100 characters'),
  description: Yup.string()
    .required('Description is required')
    .min(10, 'Description must be at least 10 characters'),
  priority: Yup.string()
    .required('Priority is required'),
  location: Yup.string()
    .required('Location is required'),
});

const CreateTicketForm: React.FC = () => {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [suggestions, setSuggestions] = useState<KeywordSuggestion[]>([]);
  const [predictedCategory, setPredictedCategory] = useState<string | null>(null);
  const [debounceTimeout, setDebounceTimeout] = useState<NodeJS.Timeout | null>(null);

  // Handle description change with debounce
  const handleDescriptionChange = (value: string, setFieldValue: (field: string, value: any) => void) => {
    setFieldValue('description', value);
    
    // Clear previous timeout
    if (debounceTimeout) {
      clearTimeout(debounceTimeout);
    }
    
    // Set new timeout
    const timeout = setTimeout(() => {
      fetchSuggestions(value);
    }, 500); // 500ms debounce
    
    setDebounceTimeout(timeout);
  };

  // Fetch keyword suggestions
  const fetchSuggestions = async (text: string) => {
    if (text.length < 5) {
      setSuggestions([]);
      setPredictedCategory(null);
      return;
    }
    
    try {
      const suggestionsData = await ticketService.getKeywordSuggestions(text);
      setSuggestions(suggestionsData);
      
      // Determine predicted category based on highest relevance
      if (suggestionsData.length > 0) {
        const categoryCounts: Record<string, number> = {};
        
        suggestionsData.forEach(suggestion => {
          if (!categoryCounts[suggestion.category]) {
            categoryCounts[suggestion.category] = 0;
          }
          categoryCounts[suggestion.category] += suggestion.relevance;
        });
        
        const predictedCat = Object.entries(categoryCounts)
          .sort((a, b) => b[1] - a[1])
          [0][0];
          
        setPredictedCategory(predictedCat);
      } else {
        setPredictedCategory(null);
      }
    } catch (err) {
      console.error('Error fetching suggestions:', err);
    }
  };

  // Apply suggestion to description
  const applySuggestion = (suggestion: KeywordSuggestion, currentText: string, setFieldValue: (field: string, value: any) => void) => {
    // Check if the keyword is already in the text
    if (currentText.toLowerCase().includes(suggestion.keyword.toLowerCase())) {
      return;
    }
    
    // Add the keyword to the end of the description
    const newText = `${currentText} ${suggestion.keyword}`;
    setFieldValue('description', newText);
    
    // Refresh suggestions
    fetchSuggestions(newText);
  };

  return (
    <Card className="shadow">
      <Card.Body>
        <h2 className="mb-4">Create New Ticket</h2>
        
        {error && <Alert variant="danger">{error}</Alert>}
        
        <Formik
          initialValues={{
            title: '',
            description: '',
            priority: 'Medium',
            location: '',
          }}
          validationSchema={TicketSchema}
          onSubmit={async (values, { setSubmitting }) => {
            try {
              const ticket = await ticketService.createTicket(values);
              navigate(`/tickets/${ticket.id}`);
            } catch (err: any) {
              setError(err.response?.data?.message || 'Failed to create ticket. Please try again.');
            } finally {
              setSubmitting(false);
            }
          }}
        >
          {({ isSubmitting, touched, errors, values, setFieldValue }) => (
            <Form>
              <BootstrapForm.Group className="mb-3">
                <BootstrapForm.Label>Title</BootstrapForm.Label>
                <Field
                  type="text"
                  name="title"
                  className={`form-control ${touched.title && errors.title ? 'is-invalid' : ''}`}
                  placeholder="Enter a descriptive title"
                />
                <ErrorMessage name="title" component="div" className="text-danger" />
              </BootstrapForm.Group>

              <BootstrapForm.Group className="mb-3">
                <BootstrapForm.Label>Description</BootstrapForm.Label>
                <Field
                  as="textarea"
                  rows={5}
                  name="description"
                  className={`form-control ${touched.description && errors.description ? 'is-invalid' : ''}`}
                  placeholder="Describe the issue in detail"
                  onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => 
                    handleDescriptionChange(e.target.value, setFieldValue)
                  }
                />
                <ErrorMessage name="description" component="div" className="text-danger" />
                
                {/* Keyword suggestions */}
                {suggestions.length > 0 && (
                  <div className="mt-2">
                    <div className="d-flex align-items-center mb-2">
                      <FontAwesomeIcon icon={faTags} className="me-2 text-primary" />
                      <strong>Suggested Keywords:</strong>
                    </div>
                    <div>
                      {suggestions.map((suggestion, index) => (
                        <Badge 
                          key={index} 
                          bg="light" 
                          text="dark" 
                          className="me-2 mb-2 p-2 border"
                          style={{ cursor: 'pointer' }}
                          onClick={() => applySuggestion(suggestion, values.description, setFieldValue)}
                        >
                          {suggestion.keyword}
                        </Badge>
                      ))}
                    </div>
                  </div>
                )}
                
                {/* Predicted category */}
                {predictedCategory && (
                  <div className="mt-3 p-2 bg-light rounded border">
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={faLightbulb} className="me-2 text-warning" />
                      <span>
                        <strong>Predicted Category:</strong> {predictedCategory}
                      </span>
                    </div>
                  </div>
                )}
              </BootstrapForm.Group>

              <BootstrapForm.Group className="mb-3">
                <BootstrapForm.Label>Priority</BootstrapForm.Label>
                <Field
                  as="select"
                  name="priority"
                  className={`form-select ${touched.priority && errors.priority ? 'is-invalid' : ''}`}
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </Field>
                <ErrorMessage name="priority" component="div" className="text-danger" />
              </BootstrapForm.Group>

              <BootstrapForm.Group className="mb-3">
                <BootstrapForm.Label>Location</BootstrapForm.Label>
                <Field
                  type="text"
                  name="location"
                  className={`form-control ${touched.location && errors.location ? 'is-invalid' : ''}`}
                  placeholder="Enter the location (e.g., Building A, Room 101)"
                />
                <ErrorMessage name="location" component="div" className="text-danger" />
              </BootstrapForm.Group>

              <div className="d-flex justify-content-between mt-4">
                <Button 
                  variant="secondary" 
                  onClick={() => navigate('/tickets')}
                >
                  Cancel
                </Button>
                <Button 
                  variant="primary" 
                  type="submit" 
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Creating...' : 'Create Ticket'}
                </Button>
              </div>
            </Form>
          )}
        </Formik>
      </Card.Body>
    </Card>
  );
};

export default CreateTicketForm;
