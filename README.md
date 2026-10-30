Student Grade Management System

1. Project Overview

The Student Grade Management System is a web-based application developed using ASP.NET Core Web API and ASP.NET Core MVC. It allows Faculty members to manage student grades while Students can securely view their own academic records.

The system uses JWT authentication and role-based authorization to ensure that users can only access features permitted for their assigned roles.

2. Technologies Used

- ASP.NET Core Web API
- ASP.NET Core MVC
- C#
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- HTML, CSS, and JavaScript
- Postman
- Visual Studio 2022

3. User Roles and Features

Faculty

- Log in to the system.
- View all student grades.
- View individual grade records.
- Add new student grades.
- Update existing grades.
- Delete grade records.

Student

- Log in to the system.
- View their own grades.
- View their own profile.
- Access only records associated with their account.

Students cannot create, update, or delete grades, and cannot access another student's records.

4. Grade Classification

The system automatically determines the remarks based on the entered numerical grade.

| Grade Range | Remarks |
|---|---|
| 95–100 | CMS |
| 90–94 | SP |
| 85–89 | SAP |
| 80–84 | GP |
| 75–79 | EP |
| 0–74 | BEP / FAILED |

Grades must be whole numbers from 0 to 100. The server determines the remarks instead of relying on values submitted by the client.

5. Project Structure

```text
StudentGradeManagement.API/
│
├── StudentGradeManagement.API/
│   ├── Controllers/
│   ├── Data/
│   │   └── IdentitySeeder.cs
│   ├── Models/
│   ├── Properties/
│   ├── Program.cs
│   └── appsettings.json
│
├── StudentGradeManagement.MVC/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── wwwroot/
│   └── Program.cs
│
├── Postman Screenshots/
├── Demo Video/
├── StudentGradeManagement.API.slnx
├── StudentGradeManagementDB_WS_FINAL_PROJECT.sql
├── Student Grade Management System API Tests.postman_collection.json
└── README.md
```

*Note: The structure above is a guide. Actual folders and files may vary depending on the final project organization.*

6. Prerequisites

Before running the application, make sure the following are installed:

- Visual Studio 2022
- .NET 8 SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Postman

7. Database Setup

The project includes an SQL script named:

`StudentGradeManagementDB_WS_FINAL_PROJECT.sql`

Follow these steps to set up the database:

1. Open SQL Server Management Studio (SSMS).
2. Connect to your local SQL Server instance.
3. Create a new database named `StudentGradeManagementDB`.
4. Open the provided SQL script.
5. Execute the SQL script to create the required tables, relationships, constraints, and initial records.
6. Verify that the tables and records have been created successfully.

**Important:** Create the database before executing the script. Execute the script only once on a newly created database unless it has been modified to support repeated execution.

8. Database Configuration

Open the API project's `appsettings.json` file.

Configure the connection string according to your local SQL Server setup.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentGradeManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

If your SQL Server uses a different server or instance name, update the connection string accordingly.

9. JWT Configuration

The API uses JWT Bearer Authentication.

The JWT signing key is stored using ASP.NET Core User Secrets instead of being placed directly in `appsettings.json`.

To configure the key:

1. Open a terminal in the API project directory.
2. Run the following commands, replacing the placeholder with a secure key:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR_SECURE_SECRET_KEY"
```

Do not commit the actual signing key or other secrets to GitHub.

10. Test Account Credentials

The system automatically creates the following test accounts through `IdentitySeeder.cs`, provided that the corresponding Faculty and Student records exist in the database.

Faculty Account

| Username | Password | Role |
|---|---|---|
| faculty01 | Faculty@123! | Faculty |

Student Accounts

| Username | Password | Role |
|---|---|---|
| student01 | Student@123! | Student |
| student02 | Student@123! | Student |
| student03 | Student@123! | Student |
| student04 | Student@123! | Student |

**Note:** These credentials are intended for academic testing only. Do not use them in a production system. Change the passwords before deploying the application publicly.

The `student04` account is intended for testing a student with no grade records.

11. Running the API Application

1. Open `StudentGradeManagement.API.slnx` in Visual Studio 2022.
2. Set `StudentGradeManagement.API` as the startup project.
3. Make sure the database and JWT User Secret are configured.
4. Run the API application.

The API Swagger page should be available at:

`https://localhost:7043/swagger/index.html`

12. Running the MVC Application

1. Open the solution in Visual Studio 2022.
2. Set `StudentGradeManagement.MVC` as the startup project.
3. Make sure the API application is running.
4. Run the MVC application.

The MVC application should be available at:

`https://localhost:7263`

13. API Endpoints

Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Auth/login` | Authenticates a user and returns a JWT token. |

Faculty Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Grades` | Retrieves all grades. |
| GET | `/api/Grades/{id}` | Retrieves a specific grade. |
| POST | `/api/Grades` | Creates a new grade record. |
| PUT | `/api/Grades/{id}` | Updates a grade record. |
| DELETE | `/api/Grades/{id}` | Deletes a grade record. |

Student Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Grades/my-grades` | Retrieves the authenticated student's grades. |
| GET | `/api/Students/my-profile` | Retrieves the authenticated student's profile. |

14. Postman Testing

The project includes a Postman collection named:

`Student Grade Management System API Tests.postman_collection.json`

Import the collection into Postman to test the API endpoints.

The collection includes tests for:

- Faculty login
- Student login
- Faculty access to all grades
- Student access to their own grades
- Authentication without a token
- Student access restrictions
- Grade creation and validation
- Grade updating and deletion
- Student record ownership
- Automatic grade remarks

Screenshots of the test results are included in the `Postman Screenshots` folder.

15. Security and Validation

The system implements the following security and validation measures:

- JWT Bearer Authentication
- Role-based authorization
- Password hashing through ASP.NET Core Identity
- Protected API endpoints
- Student record ownership verification
- Server-side grade validation
- Automatic grade remarks computation
- Prevention of unauthorized grade modifications