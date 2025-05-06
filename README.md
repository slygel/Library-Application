# Library Application

This repository contains a full-stack library management application with a .NET Core backend API and a React frontend.

## Project Structure

- `LibraryAPI/` - Backend .NET Core API
- `web-app/` - Frontend React application

## Setup and Running Instructions

### Backend (LibraryAPI)

1. Navigate to the API project directory:
   ```
   cd LibraryAPI
   ```

2. Restore NuGet packages if use vscode:
   ```
   dotnet restore
   ```

3. Update the database connection string in `LibraryAPI/LibraryAPI/appsettings.json` to match your SQL Server instance.

4. Apply database migrations:
   ```
   cd LibraryAPI
   dotnet ef database update
   ```

5. Run the API:
   ```
   dotnet run --project LibraryAPI
   ```

   The API will be available at `http://localhost:5280`.

### Frontend (web-app)

1. Navigate to the web app directory:
   ```
   cd web-app
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Start the development server:
   ```
   npm run dev
   ```

The web application will be available at `http://localhost:5173` by default.
