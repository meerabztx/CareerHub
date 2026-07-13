# 💼 Career Hub - Job Portal System

Career Hub is a full-stack **ASP.NET Core MVC (.NET 8)** web application that connects **Employers** and **Job Seekers** through a modern recruitment platform. It provides secure role-based authentication, job posting, application management, employer profiles, notifications, and an administrative dashboard.

---

## 🚀 Features

### 👨‍💼 Employer Module

* Employer Registration & Login
* Company Profile Management
* Create, Edit & Delete Job Posts
* View Posted Jobs
* View Job Applicants
* Accept / Reject Applications
* Automatic Job Seeker Notifications

### 👨‍🎓 Job Seeker Module

* User Registration & Login
* Create & Update Professional Profile
* Resume Upload
* Browse Available Jobs
* Apply for Jobs
* View Applied Jobs
* Receive Application Notifications

### 👑 Admin Module

* Secure Admin Login
* Dashboard with Statistics
* Manage Employers
* Manage Job Seekers
* Manage Jobs
* Approve / Reject Job Posts
* Manage Applications
* Monitor Platform Activity

---

## 🛠 Technologies Used

* ASP.NET Core MVC (.NET 8)
* C#
* Entity Framework Core
* ASP.NET Identity
* SQL Server LocalDB
* Bootstrap 5
* HTML5
* CSS3
* JavaScript

---

## 📂 Project Structure

```
CareerHub
│
├── Controllers
├── Models
├── Views
├── ViewModels
├── Data
├── Areas
│   └── Identity
├── wwwroot
├── Migrations
└── Program.cs
```

---

## 📊 Main Modules

* Authentication
* Employer Dashboard
* Job Seeker Dashboard
* Admin Dashboard
* Job Management
* Company Profile
* Resume Upload
* Notifications
* Role-Based Authorization

---


## ⚙️ Installation

### Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/CareerHub.git
```

### Navigate to the project

```bash
cd CareerHub
```

### Restore packages

```bash
dotnet restore
```

### Apply migrations

```bash
dotnet ef database update
```

### Run the project

```bash
dotnet run
```

---

## 🔐 User Roles

| Role       | Permissions                                          |
| ---------- | ---------------------------------------------------- |
| Admin      | Manage users, jobs, employers, applications          |
| Employer   | Manage company profile, post jobs, manage applicants |
| Job Seeker | Create profile, upload resume, apply for jobs        |

---

## 📈 Future Improvements

* Email Notifications
* Real-time Chat
* Interview Scheduling
* AI Resume Screening
* Advanced Job Search Filters
* Company Reviews
* Saved Jobs
* Password Recovery
* Two-Factor Authentication

---

## 👩‍💻 Developed By

**Meerab Asim**

Final Year Project

University of Management and Technology (UMT)

---

## 📄 License

This project is developed for educational purposes as a Final Year Project.
