import React from 'react';
import LoginForm from '../components/auth/LoginForm';
import MainLayout from '../components/layout/MainLayout';

const LoginPage: React.FC = () => {
  return (
    <MainLayout>
      <LoginForm />
    </MainLayout>
  );
};

export default LoginPage;
