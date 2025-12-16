# Fork Customizations

This folder contains customizations specific to this forked repository.

## Contents

- **run-postgres-setup.sh** - Automated setup script for PostgreSQL + Redis environment
- **SETUP_SCRIPTS_README.md** - Documentation for the setup script

## Purpose

These files are fork-specific customizations that:
- Automate local development environment setup
- Keep infrastructure files auto-generated (not tracked in git)
- Enable smooth syncing with upstream repository without merge conflicts

## Usage

```bash
# From repository root
.custom/run-postgres-setup.sh

# Or with options
.custom/run-postgres-setup.sh --skip-docker --skip-build
```

See SETUP_SCRIPTS_README.md for detailed documentation.

## Notes

- Infrastructure files (database/*.sql, docker-compose.*.yml, gateway/nginx.conf) are auto-generated and ignored in git
- This allows seamless merging with upstream changes
- No merge conflicts on infrastructure files when syncing with original repository
