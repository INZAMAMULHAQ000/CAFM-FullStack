import React, { useState } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Alert, Button, Card, Container, Form as BootstrapForm } from 'react-bootstrap';

// Validation schema
const RegisterSchema = Yup.object().shape({
  firstName: Yup.string()
    .required('First name is required')
    .min(2, 'First name must be at least 2 characters'),
  lastName: Yup.string()
    .required('Last name is required')
    .min(2, 'Last name must be at least 2 characters'),
  email: Yup.string()
    .email('Invalid email address')
    .required('Email is required'),
  password: Yup.string()
    .required('Password is required')
    .min(6, 'Password must be at least 6 characters'),
  confirmPassword: Yup.string()
    .oneOf([Yup.ref('password')], 'Passwords must match')
    .required('Confirm password is required'),
});

const RegisterForm: React.FC = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);

  return (
    <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '80vh' }}>
      <Card className="p-4 shadow" style={{ width: '500px' }}>
        <Card.Body>
          <h2 className="text-center mb-4">Register for CAFM System</h2>
          
          {error && <Alert variant="danger">{error}</Alert>}
          
          <Formik
            initialValues={{ 
              firstName: '', 
              lastName: '', 
              email: '', 
              password: '', 
              confirmPassword: '' 
            }}
            validationSchema={RegisterSchema}
            onSubmit={async (values, { setSubmitting }) => {
              try {
                await register(
                  values.email, 
                  values.password, 
                  values.firstName, 
                  values.lastName
                );
                navigate('/dashboard');
              } catch (err: any) {
                setError(err.response?.data?.message || 'Registration failed. Please try again.');
              } finally {
                setSubmitting(false);
              }
            }}
          >
            {({ isSubmitting, touched, errors }) => (
              <Form>
                <div className="row">
                  <div className="col-md-6">
                    <BootstrapForm.Group className="mb-3">
                      <BootstrapForm.Label>First Name</BootstrapForm.Label>
                      <Field
                        type="text"
                        name="firstName"
                        className={`form-control ${touched.firstName && errors.firstName ? 'is-invalid' : ''}`}
                        placeholder="Enter your first name"
                      />
                      <ErrorMessage name="firstName" component="div" className="text-danger" />
                    </BootstrapForm.Group>
                  </div>
                  <div className="col-md-6">
                    <BootstrapForm.Group className="mb-3">
                      <BootstrapForm.Label>Last Name</BootstrapForm.Label>
                      <Field
                        type="text"
                        name="lastName"
                        className={`form-control ${touched.lastName && errors.lastName ? 'is-invalid' : ''}`}
                        placeholder="Enter your last name"
                      />
                      <ErrorMessage name="lastName" component="div" className="text-danger" />
                    </BootstrapForm.Group>
                  </div>
                </div>

                <BootstrapForm.Group className="mb-3">
                  <BootstrapForm.Label>Email</BootstrapForm.Label>
                  <Field
                    type="email"
                    name="email"
                    className={`form-control ${touched.email && errors.email ? 'is-invalid' : ''}`}
                    placeholder="Enter your email"
                  />
                  <ErrorMessage name="email" component="div" className="text-danger" />
                </BootstrapForm.Group>

                <BootstrapForm.Group className="mb-3">
                  <BootstrapForm.Label>Password</BootstrapForm.Label>
                  <Field
                    type="password"
                    name="password"
                    className={`form-control ${touched.password && errors.password ? 'is-invalid' : ''}`}
                    placeholder="Enter your password"
                  />
                  <ErrorMessage name="password" component="div" className="text-danger" />
                </BootstrapForm.Group>

                <BootstrapForm.Group className="mb-3">
                  <BootstrapForm.Label>Confirm Password</BootstrapForm.Label>
                  <Field
                    type="password"
                    name="confirmPassword"
                    className={`form-control ${touched.confirmPassword && errors.confirmPassword ? 'is-invalid' : ''}`}
                    placeholder="Confirm your password"
                  />
                  <ErrorMessage name="confirmPassword" component="div" className="text-danger" />
                </BootstrapForm.Group>

                <Button 
                  variant="primary" 
                  type="submit" 
                  disabled={isSubmitting} 
                  className="w-100 mt-3"
                >
                  {isSubmitting ? 'Registering...' : 'Register'}
                </Button>
              </Form>
            )}
          </Formik>
          
          <div className="text-center mt-3">
            <p>Already have an account? <a href="/login">Login</a></p>
          </div>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default RegisterForm;
