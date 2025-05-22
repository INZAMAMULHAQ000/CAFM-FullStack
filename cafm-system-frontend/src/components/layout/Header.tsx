import React from 'react';
import { Navbar, Nav, Container, NavDropdown } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUser, faSignOutAlt, faTicketAlt, faTachometerAlt } from '@fortawesome/free-solid-svg-icons';

const Header: React.FC = () => {
  const { user, logout, isAuthenticated, hasRole } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <Navbar bg="primary" variant="dark" expand="lg" className="mb-4">
      <Container>
        <Navbar.Brand as={Link} to="/">CAFM System</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          {isAuthenticated ? (
            <>
              <Nav className="me-auto">
                <Nav.Link as={Link} to="/dashboard">
                  <FontAwesomeIcon icon={faTachometerAlt} className="me-1" /> Dashboard
                </Nav.Link>
                <Nav.Link as={Link} to="/tickets">
                  <FontAwesomeIcon icon={faTicketAlt} className="me-1" /> Tickets
                </Nav.Link>
                {hasRole('Admin') && (
                  <NavDropdown title="Admin" id="admin-dropdown">
                    <NavDropdown.Item as={Link} to="/admin/users">Users</NavDropdown.Item>
                    <NavDropdown.Item as={Link} to="/admin/settings">Settings</NavDropdown.Item>
                  </NavDropdown>
                )}
              </Nav>
              <Nav>
                <NavDropdown 
                  title={
                    <span>
                      <FontAwesomeIcon icon={faUser} className="me-1" />
                      {user?.firstName} {user?.lastName}
                    </span>
                  } 
                  id="user-dropdown"
                >
                  <NavDropdown.Item as={Link} to="/profile">Profile</NavDropdown.Item>
                  <NavDropdown.Divider />
                  <NavDropdown.Item onClick={handleLogout}>
                    <FontAwesomeIcon icon={faSignOutAlt} className="me-1" /> Logout
                  </NavDropdown.Item>
                </NavDropdown>
              </Nav>
            </>
          ) : (
            <Nav className="ms-auto">
              <Nav.Link as={Link} to="/login">Login</Nav.Link>
              <Nav.Link as={Link} to="/register">Register</Nav.Link>
            </Nav>
          )}
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default Header;
