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

echo "📥 Rebase sur SVN..."
git svn rebase

echo "📤 Envoi vers GitHub..."
git push github master

echo "✅ Synchronisation terminée !"