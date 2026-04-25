# 𝕿 Twix

A Twitter/X-style social platform built with **ASP.NET Core 8 MVC + SQLite**, deployable to Railway in one click.

---

## Features

- **Auth** — Register, Login, Logout with cookie sessions
- **Feed** — For You, Following tabs; create/edit/delete posts
- **Social** — Like, Bookmark, Follow/Unfollow users
- **Search** — Search posts and users
- **Profiles** — Public profile pages with stats
- **Admin Panel** — Full CRUD on users and posts
  - Ban / Unban users
  - Grant / Revoke admin role
  - Delete / Restore posts
  - Edit any user or post

---

## Default Credentials

| Role  | Email              | Password   |
|-------|--------------------|------------|
| Admin | admin@twix.app     | Admin123!  |
| User  | demo@twix.app      | Demo123!   |

> These are seeded automatically on first run.

---

## Run Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

```bash
# Clone / unzip the project, then:
cd Twix-Railway
dotnet run
# Open http://localhost:5000
```

The SQLite database (`twix.db`) is created automatically in the project folder on first run.

### With Docker
```bash
docker build -t twix .
docker run -p 8080:8080 -v twix-data:/data twix
# Open http://localhost:8080
```

---

## Deploy to Railway

### Option A — GitHub (recommended)

1. Push this folder to a GitHub repo
2. Go to [railway.app](https://railway.app) → **New Project** → **Deploy from GitHub repo**
3. Select your repo — Railway detects the `Dockerfile` and `railway.json` automatically
4. Add a **Volume** (optional but recommended for data persistence):
   - Mount path: `/data`
5. Click **Deploy** — done in ~2 minutes

### Option B — Railway CLI

```bash
npm install -g @railway/cli
railway login
railway init
railway up
```

### Environment Variables on Railway

| Variable  | Default         | Description                          |
|-----------|-----------------|--------------------------------------|
| `PORT`    | Set by Railway  | HTTP port (Railway sets this auto)   |
| `DB_PATH` | `/data/twix.db` | SQLite file path (use Volume at /data)|

---

## Project Structure

```
Twix-Railway/
├── Controllers/
│   ├── AccountController.cs   # Login, Register, Logout
│   ├── HomeController.cs      # Feed, Create, Like, Bookmark, Search
│   ├── ProfileController.cs   # View profile, Follow, Edit
│   └── AdminController.cs     # Full admin CRUD
├── Data/
│   ├── TwixDb.cs              # EF Core DbContext (SQLite)
│   └── DbSeeder.cs            # Seeds admin + demo user
├── Helpers/
│   └── PasswordHasher.cs      # PBKDF2-SHA256, zero dependencies
├── Models/
│   ├── User.cs
│   ├── Post.cs
│   └── SocialModels.cs        # Like, Follow, Bookmark, Notification
├── ViewModels/
│   ├── AccountViewModels.cs
│   └── PostViewModels.cs
├── Views/
│   ├── Account/               # Login, Register
│   ├── Admin/                 # Dashboard, Users, Posts, EditUser, EditPost
│   ├── Home/                  # Feed (Index)
│   ├── Profile/               # Index, Edit
│   └── Shared/                # _Layout, _PostCard
├── wwwroot/
│   ├── css/twix.css           # Full design system
│   └── js/twix.js             # Edit modal, char counter
├── Program.cs                 # App startup, auth, SQLite, Railway port
├── Twix.csproj
├── Dockerfile                 # Multi-stage build
├── railway.json               # Railway deploy config
└── .gitignore
```
