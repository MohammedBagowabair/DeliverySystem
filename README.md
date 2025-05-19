# 🚀 Go Delivery System – Smart Local Delivery Platform  

[![Blazor](https://img.shields.io/badge/Blazor-.NET%209-purple?style=for-the-badge&logo=blazor)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-UI-blueviolet?style=for-the-badge&logo=blazor)](https://mudblazor.com/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?style=for-the-badge&logo=mysql)](https://www.mysql.com/)
[![License](https://img.shields.io/badge/License-Proprietary-orange?style=for-the-badge)](LICENSE)

## 📘 Project Introduction

**Go Delivery System** is a local order and delivery management platform designed specifically for small business owners. It helps them manage daily operations efficiently and accurately—without relying on traditional paperwork.

## 🌟 Support the Project

⭐ [Star us on GitHub](https://github.com/MohammedBagowabair/DeliverySystem) — your support fuels further development!

[![LinkedIn](https://img.shields.io/badge/-Connect%20on%20LinkedIn-blue?style=flat-square&logo=linkedin)](https://www.linkedin.com/in/mohammed-bagowabair-77a0a72aa)

---

## 🏆 Key Features

### 🛠️ Core Functionality
- 📦 **Order Management** (Create/Edit/Track)
- 🚚 **Driver Assignment System**
- 📊 **Real-time Dashboard**
- 👥 **Multi-role Access Control** (Admin, Drivers, Customers)
- 📈 **Advanced Reporting** (Daily/Weekly/Monthly)

### ⚙️ Technical Highlights
- 🏗️ **Clean Architecture** Implementation
- 🧩 **CQRS Pattern** with MediatR
- 🔄 **AutoMapper** for DTO transformations
- ✅ **FluentValidation** for robust input validation
- 🗃️ **MySQL Database** Integration
- 🖥️ **Blazor WebAssembly** Client

---

## 🖼️ Visual Showcase

### 📊 Dashboard
![Dashboard](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/dashboard.png)

### 📦 Today's Orders
![Today's Orders](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/today%20orders.png)

### ➕ Add New Order
![Add Order](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/add%20order.png)

### 👤 Add Customer
![Add Customer](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/add%20customer.png)

### 🚗 Add Connector
![Add Connector](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/add%20connector.png)

### ✏️ Edit Connector
![Edit Connector](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/edit%20connector.png)

### 🔄 Change Connector
![Change Connector](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/change%20connector.png)

### 📝 Connector Details
![Connector Details](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/connector%20details.png)

### 📊 Simple Reports
![Simple Reports](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/simple%20reports.png)

### 👥 Users Management
![Users](https://github.com/MohammedBagowabair/DeliverySystem/blob/ef66079153f12f6a667ad633ec8676870a08a172/users.png)

---

## 🌐 Hosting & Availability

- 🖥️ Runs on **local hosting** with no extra cost — only a computer is required
- ☁️ **Optional online hosting** available on request

---

## 🏗️ Development Notes

- System is fully developed in **Arabic** and being translated to support **multiple languages** soon
- Currently serving **multiple active clients** and under continuous development
- Expect improvements in the dashboard, reporting, and UX in upcoming versions

---

## ⚠️ Known Limitations

- Some screens may have spelling or grammar inconsistencies due to automatic translation
- UI is optimized for Arabic display; localization support is in progress
- Further features and delivery app integrations are planned

---

## 📌 Technologies Used

- **Frontend**: Blazor, MudBlazor, HTML, CSS, JavaScript  
- **Backend**: .NET API  
- **PDF Generation**: Server-side reporting engine  
- **Database**: MySQL  
- **Architecture**: Clean Architecture with CQRS  
- **Tools**: MediatR, AutoMapper, FluentValidation

---

## 🛠️ Installation Guide

### Prerequisites
- .NET 9 SDK
- MySQL 8.0+

### Setup Instructions

1. **Clone the repositories**:
```bash
# Backend
git clone https://github.com/MohammedBagowabair/DeliverySystem.git

# Frontend
git clone https://github.com/MohammedBagowabair/DeliverySystemClientUI.git

```
## 🏗️ System Architecture

```mermaid
graph TD
    A[Presentation Layer] -->|HTTP Requests| B[API]
    B --> C[Application Layer]
    C --> D[Domain Layer]
    C --> E[Infrastructure Layer]
    E -->|MySQL| F[(Database)]
    A -->|Blazor WASM| G[Browser]
```
## 💻 Technology Stack
```mermaid
pie
    title Tech Stack Distribution
    "Blazor" : 35
    ".NET 9 API" : 30
    "MySQL Database" : 20
    "MudBlazor UI" : 10
    "Clean Architecture" : 5
```
🤝 Contributing
Pull requests are welcome! For major changes, please open an issue first to discuss.

📄 License
Proprietary - © 2024 Mohammed Bagowabair
