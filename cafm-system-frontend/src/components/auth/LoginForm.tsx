import React, { useState } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Alert, Button, Card, Container, Form as BootstrapForm } from 'react-bootstrap';

// Validation schema
const LoginSchema = Yup.object().shape({
  email: Yup.string()
    .email('Invalid email address')
    .required('Email is required'),
  password: Yup.string()
    .required('Password is required')
    .min(6, 'Password must be at least 6 characters'),
});

const LoginForm: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);

  return (
    <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '80vh' }}>
      <Card className="p-4 shadow" style={{ width: '400px' }}>
        <Card.Body>
          <h2 className="text-center mb-4">Login to CAFM System</h2>
          
          {error && <Alert variant="danger">{error}</Alert>}
          
          <Formik
            initialValues={{ email: '', password: '' }}
            validationSchema={LoginSchema}
            onSubmit={async (values, { setSubmitting }) => {
              try {
                await login(values.email, values.password);
                navigate('/dashboard');
              } catch (err: any) {
                setError(err.response?.data?.message || 'Failed to login. Please check your credentials.');
              } finally {
                setSubmitting(false);
              }
            }}
          >
            {({ isSubmitting, touched, errors }) => (
              <Form>
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

                <Button 
                  variant="primary" 
                  type="submit" 
                  disabled={isSubmitting} 
                  className="w-100 mt-3"
                >
                  {isSubmitting ? 'Logging in...' : 'Login'}
                </Button>
              </Form>
            )}
          </Formik>
          
          <div className="text-center mt-3">
            <p>Don't have an account? <a href="/register">Register</a></p>
          </div>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default LoginForm;
