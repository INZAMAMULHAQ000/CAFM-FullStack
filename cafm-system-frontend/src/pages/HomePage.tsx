import React from 'react';
import { Container, Row, Col, Card, Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTicketAlt, faBuilding, faTools, faChartLine } from '@fortawesome/free-solid-svg-icons';
import MainLayout from '../components/layout/MainLayout';

const HomePage: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return (
    <MainLayout>
      <div className="py-5">
        <Container>
          {/* Hero Section */}
          <Row className="mb-5">
            <Col md={6} className="d-flex flex-column justify-content-center">
              <h1 className="display-4 fw-bold mb-4">Facility Management Made Simple</h1>
              <p className="lead mb-4">
                Our CAFM system streamlines maintenance requests, automates ticket routing, 
                and provides real-time tracking of facility issues.
              </p>
              {isAuthenticated ? (
                <div>
                  <Button 
                    as={Link} 
                    to="/dashboard" 
                    variant="primary" 
                    size="lg" 
                    className="me-3"
                  >
                    Go to Dashboard
                  </Button>
                  <Button 
                    as={Link} 
                    to="/tickets/new" 
                    variant="outline-primary" 
                    size="lg"
                  >
                    Create Ticket
                  </Button>
                </div>
              ) : (
                <div>
                  <Button 
                    as={Link} 
                    to="/login" 
                    variant="primary" 
                    size="lg" 
                    className="me-3"
                  >
                    Login
                  </Button>
                  <Button 
                    as={Link} 
                    to="/register" 
                    variant="outline-primary" 
                    size="lg"
                  >
                    Register
                  </Button>
                </div>
              )}
            </Col>
            <Col md={6} className="mt-4 mt-md-0">
              <img 
                src="/src/assets/hero-image.svg" 
                alt="Facility Management" 
                className="img-fluid rounded shadow"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.src = 'https://via.placeholder.com/600x400?text=CAFM+System';
                }}
              />
            </Col>
          </Row>

          {/* Features Section */}
          <h2 className="text-center mb-4">Key Features</h2>
          <Row className="g-4">
            <Col md={3}>
              <Card className="h-100 shadow-sm">
                <Card.Body className="d-flex flex-column">
                  <div className="text-center mb-3">
                    <FontAwesomeIcon icon={faTicketAlt} size="3x" className="text-primary" />
                  </div>
                  <Card.Title className="text-center">Ticket Management</Card.Title>
                  <Card.Text>
                    Create, track, and manage maintenance requests with ease. Real-time updates on ticket status.
                  </Card.Text>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="h-100 shadow-sm">
                <Card.Body className="d-flex flex-column">
                  <div className="text-center mb-3">
                    <FontAwesomeIcon icon={faTools} size="3x" className="text-primary" />
                  </div>
                  <Card.Title className="text-center">Smart Routing</Card.Title>
                  <Card.Text>
                    AI-powered keyword detection automatically routes tickets to the right department.
                  </Card.Text>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="h-100 shadow-sm">
                <Card.Body className="d-flex flex-column">
                  <div className="text-center mb-3">
                    <FontAwesomeIcon icon={faBuilding} size="3x" className="text-primary" />
                  </div>
                  <Card.Title className="text-center">Facility Insights</Card.Title>
                  <Card.Text>
                    Track maintenance patterns and identify recurring issues across your facilities.
                  </Card.Text>
                </Card.Body>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="h-100 shadow-sm">
                <Card.Body className="d-flex flex-column">
                  <div className="text-center mb-3">
                    <FontAwesomeIcon icon={faChartLine} size="3x" className="text-primary" />
                  </div>
                  <Card.Title className="text-center">Analytics</Card.Title>
                  <Card.Text>
                    Comprehensive reporting and analytics to optimize maintenance operations.
                  </Card.Text>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        </Container>
      </div>
    </MainLayout>
  );
};

export default HomePage;
