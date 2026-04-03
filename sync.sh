#!/bin/bash

echo "🔄 Synchronisation SVN ↔ GitHub"
echo ""

# Va dans le dossier du projet
cd "$(dirname "$0")"

# Configure Git
git config user.name "SamuelNo"
git config user.email "samuelnoe1671@gmail.com"

echo "📥 Récupération depuis SVN..."
git svn fetch

echo "📥 Tentative de fusion (Rebase)..."
if git svn rebase; then
    echo "🚀 Tout est propre, envoi vers GitHub..."
    git push github master
else
    echo "⚠️ CONFLIT DÉTECTÉ entre ton code et le SVN !"
    echo "1. Ouvre VS Code."
    echo "2. Résous les conflits dans les fichiers soulignés en rouge."
    echo "3. Tape 'git rebase --continue' dans le terminal."
    echo "4. Relance ce script."
    exit 1
fi
echo "✅ Synchronisation terminée !"