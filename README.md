# Collaborative Task Management System

A full-stack task management application built with **Angular** and **ASP.NET Core**, designed around project-based collaboration, task workflows, role-based access, and real-time updates.

## Overview

The Collaborative Task Management System provides a workspace where users can create projects, manage boards, organize tasks, and collaborate with other members.

The application is built as a distributed full-stack system with a separate Angular frontend and multiple .NET backend services.

## Features

* Project and board management
* Workflow columns for organizing tasks
* Task creation and management
* Task comments
* Drag-and-drop task movement
* Real-time task updates using SignalR
* Project-based access control
* Role-based authorization
* JWT authentication with refresh tokens
* Google OAuth authentication
* API Gateway using YARP
* REST and gRPC communication
* Persistent task and board data
* Angular frontend with PrimeNG components

## Tech Stack

### Frontend

* Angular
* TypeScript
* RxJS
* PrimeNG
* HTML
* SCSS

### Backend

* C#
* .NET 8
* ASP.NET Core
* Entity Framework Core
* MediatR
* FluentValidation
* REST APIs
* gRPC
* SignalR
* YARP

### Database & Infrastructure

* PostgreSQL
* Redis
* JWT
* Google OAuth

### Testing & Development

* xUnit
* Jest
* Moq
* FluentAssertions
* Swagger
* Postman
* Git

## Architecture

The backend follows a layered architecture with clear separation between API, application logic, domain models, and infrastructure concerns.

```text
Collaborative-Task-Management-System
│
├── Backend
│   ├── ApiGateway
│   ├── IdentityService
│   ├── BoardTaskService
│   ├── MetadataService
│   ├── Tests
│   └── RoundTable.slnx
│
├── Frontend
│   ├── src
│   ├── public
│   ├── package.json
│   └── angular.json
│
└── README.md
```

### Backend Flow

```text
Angular Frontend
       │
       ▼
   API Gateway
      YARP
       │
       ├──────────────┐
       ▼              ▼
Identity Service   Board/Task Service
       │              │
       └───────┬──────┘
               ▼
          PostgreSQL
               │
             Redis
```

The application uses **REST APIs** for standard client-server operations and **gRPC** for service-to-service communication where appropriate.

## Authentication & Authorization

The application supports multiple authentication mechanisms:

* JWT access tokens
* Refresh tokens
* Google OAuth
* Role-based authorization
* Project-level access control

Authentication responsibilities are handled through the Identity Service, while protected requests are routed through the API Gateway.

## Real-Time Collaboration

SignalR is used to provide real-time updates between connected users.

For example, when a task is moved between workflow columns, connected clients can receive the update without requiring a full page refresh.

```text
User A
  │
  │ Move Task
  ▼
Backend
  │
  │ SignalR
  ▼
Connected Clients
  │
  ├── User B
  ├── User C
  └── User D
```

## Project Structure

### Backend

The backend is organized into multiple services:

* **ApiGateway** — routes client requests through YARP
* **IdentityService** — authentication, authorization, JWT and OAuth
* **BoardTaskService** — boards, columns, tasks and comments
* **MetadataService** — supporting application metadata

The application follows separation of concerns between the API, application, domain, and infrastructure layers.

### Frontend

The Angular application is responsible for:

* Project and board views
* Task management
* Drag-and-drop interactions
* Authentication flows
* API integration
* Real-time UI updates
* User and project access handling

## Running Locally

### Prerequisites

Make sure the following are installed:

* .NET 8 SDK
* Node.js
* Angular CLI
* PostgreSQL
* Redis

### Backend

Clone the repository:

```bash
git clone https://github.com/HinduR/Collaborative-Task-Management-System.git
cd Collaborative-Task-Management-System
```

Open the backend solution:

```text
Backend/RoundTable.slnx
```

Configure the required local settings for:

* PostgreSQL connection
* Redis
* JWT signing keys
* Google OAuth credentials

Local secrets should be stored in local configuration or environment variables and should **not** be committed to the repository.

Then build and run the required backend services.

### Frontend

Navigate to the frontend:

```bash
cd Frontend
npm install
ng serve
```

The Angular application will be available through the local development server.

## Development Approach

The project was developed with a focus on:

* Separation of concerns
* Clean API boundaries
* Reusable application services
* Secure authentication
* Clear database access patterns
* Real-time communication
* Maintainable Angular components
* Testable business logic

A typical request follows this general flow:

```text
Angular Component
       ↓
Angular Service
       ↓
REST API
       ↓
Controller
       ↓
MediatR / Application Layer
       ↓
Service / Repository
       ↓
Entity Framework Core
       ↓
PostgreSQL
```

## Current Status

The project is actively maintained as a personal full-stack development project and is used to explore practical application architecture, distributed backend services, authentication, real-time communication, and modern Angular development.

## Author

**Hindu Rajendran**

Full-Stack Developer · .NET + Angular

* GitHub: https://github.com/HinduR
* LinkedIn: https://www.linkedin.com/in/hindu-rajendran-b243a7243/

---

Build → Learn → Improve
