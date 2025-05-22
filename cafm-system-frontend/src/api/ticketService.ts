import apiClient from './axiosConfig';

export interface Ticket {
  id: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  category: string;
  location: string;
  createdBy: string;
  assignedTo?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;
  location: string;
}

export interface UpdateTicketRequest {
  id: string;
  status?: string;
  priority?: string;
  assignedTo?: string;
  category?: string;
}

export interface TicketFilterOptions {
  status?: string;
  priority?: string;
  category?: string;
  assignedTo?: string;
  createdBy?: string;
  page?: number;
  pageSize?: number;
}

export interface KeywordSuggestion {
  keyword: string;
  category: string;
  relevance: number;
}

const ticketService = {
  // Get all tickets with optional filtering
  getTickets: async (filters?: TicketFilterOptions): Promise<{ tickets: Ticket[], totalCount: number }> => {
    const queryParams = new URLSearchParams();
    
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value) {
          queryParams.append(key, value.toString());
        }
      });
    }
    
    const response = await apiClient.get<{ tickets: Ticket[], totalCount: number }>(`/tickets?${queryParams}`);
    return response.data;
  },

  // Get a single ticket by ID
  getTicketById: async (id: string): Promise<Ticket> => {
    const response = await apiClient.get<Ticket>(`/tickets/${id}`);
    return response.data;
  },

  // Create a new ticket
  createTicket: async (ticket: CreateTicketRequest): Promise<Ticket> => {
    const response = await apiClient.post<Ticket>('/tickets', ticket);
    return response.data;
  },

  // Update an existing ticket
  updateTicket: async (ticket: UpdateTicketRequest): Promise<Ticket> => {
    const response = await apiClient.put<Ticket>(`/tickets/${ticket.id}`, ticket);
    return response.data;
  },

  // Get keyword suggestions based on input text
  getKeywordSuggestions: async (text: string): Promise<KeywordSuggestion[]> => {
    const response = await apiClient.get<KeywordSuggestion[]>(`/tickets/suggestions?text=${encodeURIComponent(text)}`);
    return response.data;
  },

  // Get ticket statistics (for dashboard)
  getTicketStats: async (): Promise<any> => {
    const response = await apiClient.get('/tickets/stats');
    return response.data;
  }
};

export default ticketService;
