export interface SidebarItem {
  id: "projects" | "task-list" | "user-mapping";
  label: string;
  icon: string;
  route: string;
  requiredRole?: "Admin";
}