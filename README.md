```
# Step-by-Step Guide to Clone and Run the WebShop Project

## 1. Clone the Repository
- Open a terminal (Command Prompt, PowerShell, or any terminal of your choice).
- Navigate to the folder where you want to clone the repository.
- Run the following command to clone the repository:
  ```
  git clone https://github.com/DaniMeneses10/WebShopProject.git
  ```

- Navigate to the project directory:
  ```
  cd webshop-project
  ```

---

## 2. Set Up the Backend

### 2.1 Navigate to the Backend Folder
- Open the terminal and navigate to the backend folder:
  ```
  cd backend - WebShopApi
  ```

### 2.2 Restore NuGet Packages
- Run the following command to restore dependencies:
  ```
  dotnet restore
  ```

### 2.3 Set Up the Database
- Open the `appSettings.json` file and verify the connection string to your SQL Server database.
- Use the SQL scripts provided in the repository to create the required tables in SQL Server.

### 2.4 Run the Backend
- Start the backend application:
  ```
  dotnet run
  ```
- Ensure the backend runs successfully on `https://localhost:5001` or the specified port.

---

## 3. Set Up the Frontend

### 3.1 Navigate to the Frontend Folder
- Open the terminal and navigate to the frontend folder:
  ```
  cd webshopfrontend
  ```

### 3.2 Install npm Dependencies
- Run the following command to install all required frontend dependencies:
  ```
  npm install
  ```

### 3.3 Start the Frontend
- Start the frontend application:
  ```
  npm start
  ```
- The frontend will open automatically in your default browser at `http://localhost:3000`.

---

## 4. Test the Application

### 4.1 Test the Frontend
- Open your browser and go to `http://localhost:3000`.
- Test the following features:
  - Product catalog listing.
  - Adding items to the shopping cart.
  - Removing items from the cart.
  - Proceeding to checkout.

### 4.2 Test the Backend
- Use Swagger to test backend endpoints:
  - Go to `https://localhost:5001/swagger`.
  - Check all endpoints for correct responses.

---

## 5. Common Issues and Solutions

### 5.1 Backend Issues
- **SQL Server connection issues**:
  - Ensure SQL Server is running.
  - Verify the connection string in `appSettings.json`.

- **CORS issues**:
  - Ensure CORS is properly configured in `Program.cs` for the frontend URL (`http://localhost:3000`).

### 5.2 Frontend Issues
- **Missing dependencies**:
  - Run `npm install` again if dependencies are not found.

- **API connection issues**:
  - Ensure the backend is running on `https://localhost:5001` and accessible.

---

*Follow these steps to set up and run the WebShop project successfully. If you encounter any issues, review the troubleshooting section above.*
```