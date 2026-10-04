import { Routes } from "@angular/router";
import { authGuard } from "./Common/auth/guards/auth.guard";

export const routes: Routes = [
  {
    path: "",
    pathMatch: "full",
    redirectTo: "login",
  },
  {
    path: "login",
    loadComponent: () =>
      import("./Components/login/login.component/login.component").then(
        (component) => component.LoginComponent,
      ),
  },
   {
    path: 'auth/callback',
    loadComponent: () =>
      import(
        './Components/call-back/auth-callback.component'
      ).then(
        (component) =>
          component.AuthCallbackComponent,
      ),
  },
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./layout/main-layout/main-layout.component").then(
        (component) => component.MainLayoutComponent,
      ),
    children: [
      {
        path: "projects",
        loadComponent: () =>
          import(
            "./Components/project/project.component/project.component"
          ).then((component) => component.ProjectComponent),
      },
      {
        path: "projects/:projectId/boards",
        loadComponent: () =>
          import(
            "./Components/board-management/component/board.component/board.component"
          ).then((component) => component.BoardComponent),
      },
      {
        path: "projects/:projectId/boards/:boardId",
        loadComponent: () =>
          import(
            "./Components/board-management/component/board-view.component/board-view.component"
          ).then((component) => component.BoardViewComponent),
      },
      {
  path: "projects/:projectId/boards/:boardId/tasks",
  loadComponent: () =>
    import(
      "./Components/board-management/component/task-list.component/task-list.component"
    ).then((component) => component.TaskListComponent),
},
      {
        path: "admin/user-mapping",
        loadComponent: () =>
          import(
            "./Components/user-mapping/component/user-mapping.component"
          ).then((component) => component.UserMappingComponent),
      },
    ],
  },
  {
    path: "**",
    redirectTo: "login",
  },
];
