import React from 'react';
import CreateTicketForm from '../components/tickets/CreateTicketForm';
import MainLayout from '../components/layout/MainLayout';

const CreateTicketPage: React.FC = () => {
  return (
    <MainLayout>
      <h1 className="mb-4">Create New Ticket</h1>
      <CreateTicketForm />
    </MainLayout>
  );
};

export default CreateTicketPage;
