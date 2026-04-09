# ✅ Checklist de Buenas Prácticas - Faeterna

Antes de hacer **push** a GitHub, verifica que hayas seguido estas buenas prácticas:

---

## 📁 **Archivos de Configuración**

- ✅ `.gitignore` - Configurado para ignorar archivos innecesarios
- ✅ `.gitattributes` - Normaliza line endings (LF)
- ✅ `.editorconfig` - Consistencia de indentación y espacios
- ✅ `.vscode/settings.json` - Configuración recomendada de editor

---

## 🚫 **NUNCA commits estos archivos**

```
bin/                 # Archivos compilados
obj/                 # Objetos de compilación
.godot/              # Cache de Godot
.vs/                 # Visual Studio user data
.idea/               # Rider user data
*.user               # Visual Studio user files
*.log                # Archivos de log
.env*                # Variables de entorno
```

Verifica con:
```bash
git status
```

---

## 👤 **Identidad de Git (¡importante para las contribuciones!)**

Asegúrate de que tus commits quedan atribuidos a **tu** cuenta de GitHub:

```bash
git config --global user.name   # debe ser tu usuario de GitHub
git config --global user.email  # debe ser el email verificado en tu cuenta
```

Si ves `Faeterna Bot` o un email como `faeterna-bot@example.local`, corrígelo:

```bash
git config --global user.name  "TuUsuarioDeGitHub"
git config --global user.email "TuUsuario@users.noreply.github.com"
```

> Consulta [`docs/GIT_AUTHOR_SETUP.md`](GIT_AUTHOR_SETUP.md) para instrucciones detalladas.

---

## 🎯 **Antes de hacer Commit**

- [ ] He compilado el proyecto exitosamente
- [ ] Mis cambios no generan warnings
- [ ] Ejecuté los tests (si hay)
- [ ] Revisé con `git diff` mis cambios
- [ ] No incluyo archivos de `.gitignore`
- [ ] Verifiqué que `git config user.email` es mi email de GitHub

```bash
# Verificar qué va a subirse
git add --dry-run .
git status
```

---

## 📝 **Mensaje de Commit**

- ✅ Usa presente: "Add feature" no "Added feature"
- ✅ Sé específico: "Add player jump mechanic" no "Fix stuff"
- ✅ Máx 50 caracteres en el título
- ✅ Si es complejo, añade descripción en el body

**Ejemplo:**
```
Add double jump ability to movement system

- Implement DoubleJumpMovementState
- Update MovementStateMachine with new state
- Add animation transitions for double jump
- Update player input handling

Fixes #42
```

---

## 🔍 **Verificación de Código**

- [ ] Los archivos `.cs` siguen convenciones C# (PascalCase para clases)
- [ ] Los scripts `.gd` siguen convenciones GDScript (snake_case)
- [ ] El código está documentado (comentarios en métodos complejos)
- [ ] No hay código comentado (elimínalo o haz una rama)
- [ ] No hay variables no utilizadas

---

## 📦 **Si modificas Faeterna.csproj**

- [ ] No incluyas rutas absolutas
- [ ] Usa rutas relativas
- [ ] Sincroniza con otros devs si haces cambios importantes

---

## 📄 **Si añades Assets**

- [ ] Los sprites están en `/assets/` en carpetas organizadas
- [ ] Los nombres de archivos son descriptivos
- [ ] Están en formato correcto (PNG para sprites, OGG para audio)
- [ ] No subas archivos de proyecto sin comprimir (`.psd`, `.ai`, etc.)

---

## 🎮 **Si creas nuevas Escenas**

- [ ] Están en `/scenes/` en la carpeta correspondiente
- [ ] Nombres claros y descriptivos
- [ ] Scripts asociados están en `/scripts/`
- [ ] Has hecho una prueba rápida en Godot

---

## 🆕 **Si creas nuevos Scripts**

- [ ] Están en `/scripts/` con estructura clara
- [ ] El nombre del archivo coincide con el nombre de la clase
- [ ] Incluyen docstrings (comentarios de método)
- [ ] Siguen la arquitectura existente

**Ejemplo C#:**
```csharp
/// <summary>
/// Manages player movement including running, jumping, and dashing.
/// </summary>
public class Lira : Node2D
{
    /// <summary>
    /// The current movement state of the player.
    /// </summary>
    private State _currentState;
}
```

---

## 🧪 **Testing**

- [ ] El juego se compila sin errores
- [ ] El juego corre sin crashes
- [ ] Probaste tu feature específicamente
- [ ] No rompiste features existentes

```bash
# Compilar desde terminal
dotnet build
```

---

## 🔐 **Sensibilidad de Datos**

- [ ] No incluyo credenciales
- [ ] No incluyo API keys
- [ ] No incluyo información personal
- [ ] No incluyo archivos `.env` o `.env.local`

**Si necesitas variables de configuración local:**
```
1. Crea un .env.example con placeholders
2. Gitignora el .env real
3. Documenta en README.md qué necesita el dev
```

---

## 📊 **Gran cambio o Feature Nueva?**

1. **Crea una rama:**
   ```bash
   git checkout -b feature/descripcion
   ```

2. **Haz commits pequeños y lógicos**
   - No hagas 1 commit con 50 cambios

3. **Abre un Pull Request**
   - Describe qué hace
   - Menciona issues relacionados
   - Pide revisión

4. **Espera feedback**
   - Responde comentarios
   - Haz los cambios solicitados
   - Actualiza el PR

---

## 🎓 **Referencias Rápidas**

- [Godot Best Practices](https://docs.godotengine.org/en/stable/)
- [C# Style Guide](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- [Git Best Practices](https://git-scm.com/book/en/v2)
- [GitHub Flow](https://guides.github.com/introduction/flow/)

---

## ❓ **¿Dudas?**

Consulta `CONTRIBUTING_GITIGNORE.md` para detalles sobre qué no debe subirse.

---

**¡Gracias por seguir las buenas prácticas!** 🙏
