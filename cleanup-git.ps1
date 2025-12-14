# Git Cleanup Script
# Премахва вече качени файлове, които сега са в .gitignore

# IMPORTANT: Изпълни тези команди в Git Bash или PowerShell с Git инсталиран
# Файловете ще бъдат премахнати само от Git кеша, но НЕ и от диска

# 1. Премахни чувствителни конфигурационни файлове
git rm --cached backend/Petoria/appsettings.json

# 2. Премахни node_modules (ако е качен)
git rm --cached -r frontend/node_modules 2>$null

# 3. Премахни package-lock.json
git rm --cached frontend/package-lock.json 2>$null

# 4. Премахни .vs директорията (Visual Studio cache)
git rm --cached -r backend/.vs 2>$null

# 5. Премахни bin и obj папки
git rm --cached -r backend/Petoria/bin 2>$null
git rm --cached -r backend/Petoria/obj 2>$null
git rm --cached -r backend/Petoria.Core/bin 2>$null
git rm --cached -r backend/Petoria.Core/obj 2>$null
git rm --cached -r backend/Petoria.Infrastructure/bin 2>$null
git rm --cached -r backend/Petoria.Infrastructure/obj 2>$null
git rm --cached -r backend/Petoria.Constants/bin 2>$null
git rm --cached -r backend/Petoria.Constants/obj 2>$null

# 6. Премахни frontend dist папка
git rm --cached -r frontend/dist 2>$null

# 7. След това направи commit
# git add .
# git commit -m "Update .gitignore and remove sensitive/unnecessary files"

Write-Host "Cleanup commands completed!"
Write-Host ""
Write-Host "ВАЖНО: Преди да качиш промените в GitHub:"
Write-Host "1. Провери какво ще бъде committed: git status"
Write-Host "2. Направи commit: git commit -m 'Update .gitignore and remove sensitive files'"
Write-Host "3. Push промените: git push"
Write-Host ""
Write-Host "Файлът backend/Petoria/appsettings.json ще остане на диска, но няма да бъде tracked от Git."
