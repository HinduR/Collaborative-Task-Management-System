export interface AuthenticatedUser {
  id: string;
  email: string;
  userName: string;
  roles: string[];
  features: string[];
}