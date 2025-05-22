import React from 'react';
import TicketList from '../components/tickets/TicketList';
import MainLayout from '../components/layout/MainLayout';

const TicketsPage: React.FC = () => {
  return (
    <MainLayout>
      <TicketList />
    </MainLayout>
  );
};

export default TicketsPage;
