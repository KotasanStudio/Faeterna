# 🔧 Configuración de identidad Git para contribuidores

Este documento explica cómo asegurarte de que tus commits aparecen correctamente atribuidos a tu cuenta de GitHub.

---

## ¿Por qué es importante?

GitHub muestra las contribuciones de cada persona en su perfil basándose en el **email del autor del commit**. Si el email del commit no está asociado (y verificado) a tu cuenta de GitHub, tus commits no contarán como contribuciones tuyas y pueden aparecer bajo un nombre genérico (como `Faeterna Bot`).

---

## Pasos para configurar tu identidad correctamente

### 1. Configura tu nombre y email de Git

Abre una terminal y ejecuta:

```bash
git config --global user.name  "HaolinZheng"
git config --global user.email "HaolinZheng@users.noreply.github.com"
```

> **Nota:** `HaolinZheng@users.noreply.github.com` es el email privado de GitHub. Lo puedes encontrar en  
> **GitHub → Settings → Emails → Keep my email addresses private**.  
> Si prefieres usar tu email real, asegúrate de que esté **verificado** en tu cuenta de GitHub.

### 2. Verifica la configuración

```bash
git config --global user.name
git config --global user.email
```

También revisa si el repositorio tiene una configuración local que la sobreescriba (dentro de la carpeta del proyecto):

```bash
git config user.name
git config user.email
```

Si la configuración local es incorrecta, corrígela:

```bash
git config user.name  "HaolinZheng"
git config user.email "HaolinZheng@users.noreply.github.com"
```

### 3. Añade el email a tu cuenta de GitHub

1. Ve a **GitHub → Settings → Emails**.
2. Añade `HaolinZheng@users.noreply.github.com` (o tu email real) si no está ya añadido.
3. Verifícalo si es necesario.

---

## ¿Cómo sé si mis commits están bien atribuidos?

Después de hacer un push, accede al commit en GitHub y comprueba que tu avatar/perfil aparece como autor. Si sale un avatar vacío o el nombre "Faeterna Bot", repite los pasos anteriores.

---

## Guardrail automático (CI)

Existe un workflow de GitHub Actions (`.github/workflows/check-commit-author.yml`) que **falla automáticamente** si detecta commits con la identidad `Faeterna Bot` / `faeterna-bot@example.local`. Esto evita que el problema reaparezca.

---

## Contexto del problema detectado

Se identificaron commits con:
- **Nombre:** `Faeterna Bot`
- **Email:** `faeterna-bot@example.local`

Estos commits **no están vinculados a ninguna cuenta de GitHub**, por lo que las contribuciones de **HaolinZheng** no aparecían correctamente en el proyecto. Aplicando la configuración anterior, todos los commits futuros quedarán correctamente atribuidos.
