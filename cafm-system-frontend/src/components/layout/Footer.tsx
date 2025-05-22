import React from 'react';
import { Container } from 'react-bootstrap';

const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();
  
  return (
    <footer className="bg-light py-3 mt-auto">
      <Container>
        <div className="text-center">
          <p className="mb-0">
            &copy; {currentYear} CAFM System. All rights reserved.
          </p>
        </div>
      </Container>
    </footer>
  );
};

export default Footer;
