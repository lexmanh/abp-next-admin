# 🚀 ABP Next Admin - PostgreSQL Automation Scripts

> **Automated setup scripts to run ABP Next Admin with PostgreSQL after every sync with the remote repository**

---

## 📁 Files in This Package

| File | Size | Purpose |
|------|------|---------|
| **run-postgres-setup.sh** | 12 KB | Shell script (macOS/Linux) - **START HERE** |
| **run-postgres-setup.py** | 16 KB | Python script (cross-platform) |
| **QUICK_REFERENCE.md** | 4.2 KB | Quick command lookup & troubleshooting |
| **POSTGRES_SETUP_GUIDE.md** | 6.5 KB | Complete detailed guide |

---

## ⚡ One-Command Setup

```bash
./run-postgres-setup.sh
```

That's it! This single command:
- ✅ Creates PostgreSQL 15 container
- ✅ Creates Redis 7 container
- ✅ Creates `Platform-V70` database
- ✅ Builds the entire solution
- ✅ Runs database migrations
- ✅ Starts the application
- ✅ Verifies everything works

**Result**: Application runs on http://127.0.0.1:30001

---

## 🔄 After Syncing with Remote Repository

```bash
# Most common: quick restart after "git pull"
./run-postgres-setup.sh --skip-docker --skip-build

# If code changed significantly
./run-postgres-setup.sh --skip-docker

# For fresh database
./run-postgres-setup.sh --clean-containers
```

---

## 📊 Script Comparison

| Feature | Shell Script | Python Script |
|---------|:---:|:---:|
| Speed | ⚡ Faster | Medium |
| macOS/Linux | ✅ Native | ✅ Works |
| Windows | ❌ Needs WSL | ✅ Works |
| Dependencies | Minimal | Needs Python 3.6+ |
| Error Messages | Good | Excellent |
| Recommended | ✅ Yes | Alternative |

---

## 📖 Choose Your Guide

### For Quick Commands
👉 Read: **QUICK_REFERENCE.md**
- Most common commands
- Troubleshooting table
- Quick lookup

### For Detailed Instructions
👉 Read: **POSTGRES_SETUP_GUIDE.md**
- Full feature list
- All options explained
- Scenarios & examples
- Manual steps

### For Help Text
```bash
./run-postgres-setup.sh --help
```

---

## ✨ Key Features

- **Smart**: Skips unnecessary steps (checks if containers already exist)
- **Fast**: ~2-4 minutes for subsequent runs
- **Safe**: Validates everything before running
- **Helpful**: Colored output, detailed logging, clear error messages
- **Flexible**: Multiple command-line options for different scenarios
- **Documented**: Comprehensive guides included

---

## 🎯 Common Workflows

### Workflow 1: Fresh Installation
```bash
./run-postgres-setup.sh
# Takes 8-12 minutes first time
```

### Workflow 2: After Git Pull (Most Common)
```bash
git pull origin dev
./run-postgres-setup.sh --skip-docker --skip-build
# Takes 1-2 minutes
```

### Workflow 3: Only Rebuild Needed
```bash
./run-postgres-setup.sh --skip-docker
# Takes 2-3 minutes
```

### Workflow 4: Fresh Database
```bash
./run-postgres-setup.sh --clean-containers
# Removes old containers and creates fresh ones
```

---

## 🔧 What Gets Set Up

```
Setup Phase 1: Validation
├─ Check Docker installed
├─ Check .NET SDK installed
├─ Verify project structure
└─ ✓ Ready to proceed

Setup Phase 2: Docker
├─ Create PostgreSQL 15 container (port 5432)
├─ Create Redis 7 container (port 6379)
├─ Create Platform-V70 database
└─ ✓ Containers running

Setup Phase 3: Build
├─ Run: dotnet build LINGYUN.MicroService.SingleProject.sln
└─ ✓ All projects compiled

Setup Phase 4: Migrations
├─ Run database migrator
├─ Initialize schema
└─ ✓ Database ready

Setup Phase 5: Startup
├─ Start application on port 30001
├─ Wait for startup completion
├─ Verify Swagger UI accessible
└─ ✓ Application running!
```

---

## 📋 Default Configuration

| Setting | Value |
|---------|-------|
| **Database** | Platform-V70 |
| **DB Host** | 127.0.0.1 |
| **DB Port** | 5432 |
| **DB User** | postgres |
| **DB Password** | 123456 |
| **Redis Port** | 6379 |
| **App Port** | 30001 |
| **Environment** | Development |

*To change these, edit the configuration section in the script*

---

## ✅ Verification

After setup, verify everything:

```bash
# Check application is running
curl -s http://127.0.0.1:30001/swagger/index.html | head -5

# Check containers
docker ps | grep -E "postgres|redis"

# Check application logs
tail -50 /tmp/abp-app.log
```

**Success**: You should see:
- Swagger HTML output
- Both postgres-abp and redis-abp containers
- "Application is running successfully" in logs

---

## 🚨 Troubleshooting

### Problem: Port 30001 already in use
```bash
./run-postgres-setup.sh --clean-containers
# Or edit script and change APP_PORT
```

### Problem: Docker not running
```bash
# Start Docker Desktop, then run:
./run-postgres-setup.sh
```

### Problem: Build fails
```bash
# Full clean rebuild:
./run-postgres-setup.sh --clean-containers
```

### Problem: Database connection error
```bash
# Check PostgreSQL is running:
docker logs postgres-abp

# Restart containers:
./run-postgres-setup.sh --clean-containers
```

**More help**: See **POSTGRES_SETUP_GUIDE.md** troubleshooting section

---

## 📍 Directory Structure

```
abp-next-admin/
├── run-postgres-setup.sh       ← Use this
├── run-postgres-setup.py       ← Or this
├── QUICK_REFERENCE.md          ← Quick help
├── POSTGRES_SETUP_GUIDE.md     ← Full guide
└── aspnet-core/
    ├── LINGYUN.MicroService.SingleProject.sln
    ├── services/
    │   └── LY.MicroService.Applications.Single/
    └── migrations/
        └── LY.MicroService.Applications.Single.DbMigrator/
```

---

## 🎓 First-Time Setup Guide

1. **Navigate to repository root**
   ```bash
   cd /Users/lxmanh/Projects/lexmanh/abp-next-admin
   ```

2. **Run the setup script**
   ```bash
   ./run-postgres-setup.sh
   ```

3. **Wait for completion** (8-12 minutes first time)
   - Watch the colored output for progress
   - Script will verify at the end

4. **Access the application**
   - Swagger UI: http://127.0.0.1:30001/swagger/index.html
   - API Base: http://127.0.0.1:30001

5. **Check logs if needed**
   ```bash
   tail -100 /tmp/abp-app.log
   ```

---

## 🔐 Security Notes

⚠️ **Development Only**:
- Default password `123456` is not secure
- Redis has no authentication
- SSL is disabled for OpenIddict

For production use, update the configuration in appsettings files.

---

## 📞 Need Help?

1. **Quick lookup**: `cat QUICK_REFERENCE.md`
2. **Detailed guide**: `cat POSTGRES_SETUP_GUIDE.md`
3. **Script help**: `./run-postgres-setup.sh --help`
4. **Check logs**: `tail /tmp/abp-app.log`

---

## 🎯 Pro Tips

- Use `--skip-docker --skip-build` after git pull for fastest startup
- Keep docker containers running between sessions for speed
- Check `/tmp/abp-app.log` first when debugging
- The scripts are idempotent - safe to run multiple times

---

## 📊 Performance Expectations

| Scenario | Time |
|----------|------|
| First run | 8-12 min |
| Fresh build | 5-8 min |
| Fresh migrations | 1-2 min |
| App startup | 20-30 sec |
| **Skip Docker & Build** | **1-2 min** ← Most common |
| **Skip Everything** | **< 1 min** |

---

## 🚀 Ready to Begin?

```bash
./run-postgres-setup.sh
```

Sit back and let the script handle everything! ☕

---

**Created**: December 16, 2025  
**Framework**: ABP v9.3.6 on .NET 9.0  
**Database**: PostgreSQL 15-alpine + Redis 7-alpine  
**Target**: macOS / Linux (with Python alternative for cross-platform)
