import React, { useState, useEffect } from 'react';
import { Row, Col, Card, Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faTicketAlt, 
  faSpinner, 
  faCheckCircle, 
  faTimesCircle,
  faExclamationTriangle,
  faPlus
} from '@fortawesome/free-solid-svg-icons';
import MainLayout from '../components/layout/MainLayout';
import ticketService from '../api/ticketService';
import { useAuth } from '../context/AuthContext';

interface TicketStats {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  completedTickets: number;
  closedTickets: number;
  criticalTickets: number;
  ticketsByCategory: Record<string, number>;
}

const DashboardPage: React.FC = () => {
  const { user, hasRole } = useAuth();
  const [stats, setStats] = useState<TicketStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    try {
      const data = await ticketService.getTicketStats();
      setStats(data);
    } catch (err: any) {
      setError(err.message || 'Failed to fetch dashboard data');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="d-flex justify-content-center align-items-center" style={{ height: '70vh' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (error) {
    return (
      <MainLayout>
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="mb-4 d-flex justify-content-between align-items-center">
        <h1>Dashboard</h1>
        <Button 
          as={Link} 
          to="/tickets/new" 
          variant="primary"
        >
          <FontAwesomeIcon icon={faPlus} className="me-2" />
          Create Ticket
        </Button>
      </div>

      <Row className="mb-4">
        <Col md={3}>
          <Card className="text-center h-100 shadow-sm">
            <Card.Body>
              <div className="display-4 text-primary mb-2">
                {stats?.totalTickets || 0}
              </div>
              <Card.Title>Total Tickets</Card.Title>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="text-center h-100 shadow-sm">
            <Card.Body>
              <div className="display-4 text-primary mb-2">
                {stats?.openTickets || 0}
              </div>
              <Card.Title>Open Tickets</Card.Title>
              <FontAwesomeIcon icon={faTicketAlt} className="text-primary" />
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="text-center h-100 shadow-sm">
            <Card.Body>
              <div className="display-4 text-warning mb-2">
                {stats?.inProgressTickets || 0}
              </div>
              <Card.Title>In Progress</Card.Title>
              <FontAwesomeIcon icon={faSpinner} className="text-warning" />
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="text-center h-100 shadow-sm">
            <Card.Body>
              <div className="display-4 text-success mb-2">
                {stats?.completedTickets || 0}
              </div>
              <Card.Title>Completed</Card.Title>
              <FontAwesomeIcon icon={faCheckCircle} className="text-success" />
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Row className="mb-4">
        <Col md={6}>
          <Card className="h-100 shadow-sm">
            <Card.Header>
              <h5 className="mb-0">Tickets by Category</h5>
            </Card.Header>
            <Card.Body>
              {stats?.ticketsByCategory && Object.keys(stats.ticketsByCategory).length > 0 ? (
                <div>
                  {Object.entries(stats.ticketsByCategory).map(([category, count]) => (
                    <div key={category} className="mb-3">
                      <div className="d-flex justify-content-between mb-1">
                        <span>{category}</span>
                        <span>{count}</span>
                      </div>
                      <div className="progress">
                        <div 
                          className="progress-bar" 
                          role="progressbar" 
                          style={{ 
                            width: `${(count / stats.totalTickets) * 100}%`,
                            backgroundColor: 
                              category === 'Plumbing' ? '#0d6efd' :
                              category === 'Electrical' ? '#ffc107' :
                              category === 'HVAC' ? '#20c997' :
                              category === 'Cleaning' ? '#6f42c1' : '#6c757d'
                          }} 
                          aria-valuenow={(count / stats.totalTickets) * 100} 
                          aria-valuemin={0} 
                          aria-valuemax={100}
                        ></div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-center text-muted">No category data available</p>
              )}
            </Card.Body>
          </Card>
        </Col>
        <Col md={6}>
          <Card className="h-100 shadow-sm">
            <Card.Header>
              <h5 className="mb-0">Critical Issues</h5>
            </Card.Header>
            <Card.Body>
              {stats?.criticalTickets ? (
                <div className="d-flex align-items-center">
                  <div className="display-1 me-3 text-danger">
                    <FontAwesomeIcon icon={faExclamationTriangle} />
                  </div>
                  <div>
                    <h2 className="mb-0">{stats.criticalTickets}</h2>
                    <p className="mb-0">Critical tickets require immediate attention</p>
                    <Button 
                      as={Link} 
                      to="/tickets?priority=Critical" 
                      variant="outline-danger" 
                      className="mt-3"
                    >
                      View Critical Tickets
                    </Button>
                  </div>
                </div>
              ) : (
                <div className="text-center py-4">
                  <FontAwesomeIcon icon={faCheckCircle} size="3x" className="text-success mb-3" />
                  <h4>No Critical Issues</h4>
                  <p className="text-muted">All systems are running smoothly</p>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Row>
        <Col md={12}>
          <Card className="shadow-sm">
            <Card.Header>
              <h5 className="mb-0">Quick Actions</h5>
            </Card.Header>
            <Card.Body>
              <div className="d-flex flex-wrap gap-2">
                <Button as={Link} to="/tickets" variant="outline-primary">
                  View All Tickets
                </Button>
                <Button as={Link} to="/tickets/new" variant="outline-success">
                  Create New Ticket
                </Button>
                {hasRole('Admin') && (
                  <>
                    <Button as={Link} to="/admin/users" variant="outline-secondary">
                      Manage Users
                    </Button>
                    <Button as={Link} to="/admin/settings" variant="outline-secondary">
                      System Settings
                    </Button>
                  </>
                )}
                {hasRole('AssetManager') && (
                  <Button as={Link} to="/tickets?status=Open" variant="outline-warning">
                    Assign Open Tickets
                  </Button>
                )}
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </MainLayout>
  );
};

export default DashboardPage;
