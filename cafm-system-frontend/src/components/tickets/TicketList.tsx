import React, { useState, useEffect } from 'react';
import { Table, Badge, Button, Form, Row, Col, Pagination } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import ticketService, { Ticket, TicketFilterOptions } from '../../api/ticketService';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEye, faFilter, faSortAmountDown } from '@fortawesome/free-solid-svg-icons';

const TicketList: React.FC = () => {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [filters, setFilters] = useState<TicketFilterOptions>({
    page: 1,
    pageSize: 10,
  });

  // Status options
  const statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Open', label: 'Open' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Closed', label: 'Closed' },
  ];

  // Priority options
  const priorityOptions = [
    { value: '', label: 'All Priorities' },
    { value: 'Low', label: 'Low' },
    { value: 'Medium', label: 'Medium' },
    { value: 'High', label: 'High' },
    { value: 'Critical', label: 'Critical' },
  ];

  // Category options
  const categoryOptions = [
    { value: '', label: 'All Categories' },
    { value: 'Plumbing', label: 'Plumbing' },
    { value: 'Electrical', label: 'Electrical' },
    { value: 'HVAC', label: 'HVAC' },
    { value: 'Cleaning', label: 'Cleaning' },
    { value: 'Other', label: 'Other' },
  ];

  useEffect(() => {
    fetchTickets();
  }, [filters]);

  const fetchTickets = async () => {
    setLoading(true);
    try {
      const response = await ticketService.getTickets(filters);
      setTickets(response.tickets);
      setTotalCount(response.totalCount);
    } catch (err: any) {
      setError(err.message || 'Failed to fetch tickets');
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFilters(prev => ({
      ...prev,
      [name]: value,
      page: 1, // Reset to first page when filter changes
    }));
  };

  const handlePageChange = (page: number) => {
    setFilters(prev => ({
      ...prev,
      page,
    }));
  };

  // Get status badge variant
  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Open':
        return 'primary';
      case 'InProgress':
        return 'warning';
      case 'Completed':
        return 'success';
      case 'Closed':
        return 'secondary';
      default:
        return 'info';
    }
  };

  // Get priority badge variant
  const getPriorityBadge = (priority: string) => {
    switch (priority) {
      case 'Low':
        return 'info';
      case 'Medium':
        return 'secondary';
      case 'High':
        return 'warning';
      case 'Critical':
        return 'danger';
      default:
        return 'light';
    }
  };

  // Calculate pagination
  const totalPages = Math.ceil(totalCount / (filters.pageSize || 10));
  const currentPage = filters.page || 1;

  return (
    <div>
      <h2 className="mb-4">Tickets</h2>
      
      {/* Filters */}
      <div className="mb-4 p-3 border rounded bg-light">
        <h5>
          <FontAwesomeIcon icon={faFilter} className="me-2" />
          Filters
        </h5>
        <Row>
          <Col md={3}>
            <Form.Group className="mb-3">
              <Form.Label>Status</Form.Label>
              <Form.Select 
                name="status" 
                value={filters.status || ''} 
                onChange={handleFilterChange}
              >
                {statusOptions.map(option => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          <Col md={3}>
            <Form.Group className="mb-3">
              <Form.Label>Priority</Form.Label>
              <Form.Select 
                name="priority" 
                value={filters.priority || ''} 
                onChange={handleFilterChange}
              >
                {priorityOptions.map(option => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          <Col md={3}>
            <Form.Group className="mb-3">
              <Form.Label>Category</Form.Label>
              <Form.Select 
                name="category" 
                value={filters.category || ''} 
                onChange={handleFilterChange}
              >
                {categoryOptions.map(option => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          <Col md={3} className="d-flex align-items-end">
            <Button 
              variant="primary" 
              className="mb-3 w-100"
              as={Link}
              to="/tickets/new"
            >
              Create New Ticket
            </Button>
          </Col>
        </Row>
      </div>

      {/* Error message */}
      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      {/* Loading indicator */}
      {loading ? (
        <div className="text-center my-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      ) : (
        <>
          {/* Tickets table */}
          <Table striped bordered hover responsive>
            <thead>
              <tr>
                <th>ID</th>
                <th>Title</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Category</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {tickets.length === 0 ? (
                <tr>
                  <td colSpan={7} className="text-center">No tickets found</td>
                </tr>
              ) : (
                tickets.map(ticket => (
                  <tr key={ticket.id}>
                    <td>{ticket.id.substring(0, 8)}...</td>
                    <td>{ticket.title}</td>
                    <td>
                      <Badge bg={getStatusBadge(ticket.status)}>
                        {ticket.status}
                      </Badge>
                    </td>
                    <td>
                      <Badge bg={getPriorityBadge(ticket.priority)}>
                        {ticket.priority}
                      </Badge>
                    </td>
                    <td>{ticket.category}</td>
                    <td>{new Date(ticket.createdAt).toLocaleDateString()}</td>
                    <td>
                      <Button 
                        variant="outline-primary" 
                        size="sm"
                        as={Link}
                        to={`/tickets/${ticket.id}`}
                      >
                        <FontAwesomeIcon icon={faEye} /> View
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </Table>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="d-flex justify-content-center mt-4">
              <Pagination>
                <Pagination.First 
                  onClick={() => handlePageChange(1)} 
                  disabled={currentPage === 1} 
                />
                <Pagination.Prev 
                  onClick={() => handlePageChange(currentPage - 1)} 
                  disabled={currentPage === 1} 
                />
                
                {Array.from({ length: totalPages }, (_, i) => i + 1)
                  .filter(page => 
                    page === 1 || 
                    page === totalPages || 
                    Math.abs(page - currentPage) <= 2
                  )
                  .map((page, index, array) => {
                    // Add ellipsis
                    if (index > 0 && page - array[index - 1] > 1) {
                      return (
                        <React.Fragment key={`ellipsis-${page}`}>
                          <Pagination.Ellipsis disabled />
                          <Pagination.Item
                            active={page === currentPage}
                            onClick={() => handlePageChange(page)}
                          >
                            {page}
                          </Pagination.Item>
                        </React.Fragment>
                      );
                    }
                    return (
                      <Pagination.Item
                        key={page}
                        active={page === currentPage}
                        onClick={() => handlePageChange(page)}
                      >
                        {page}
                      </Pagination.Item>
                    );
                  })}
                
                <Pagination.Next 
                  onClick={() => handlePageChange(currentPage + 1)} 
                  disabled={currentPage === totalPages} 
                />
                <Pagination.Last 
                  onClick={() => handlePageChange(totalPages)} 
                  disabled={currentPage === totalPages} 
                />
              </Pagination>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default TicketList;
