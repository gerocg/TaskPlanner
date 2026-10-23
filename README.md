TaskPlanner - WPF Task Manager

Una aplicación de escritorio ligera y moderna desarrollada en **.NET 8.0 y WPF** para la gestión y planificación diaria de tareas, orientada a la productividad y la simplicidad.

---

**Aviso de alcance:** Este proyecto fue desarrollado con fines exclusivamente **personales y de práctica autodidacta** para explorar la integración de WPF con APIs nativas de Windows, persistencia ligera en disco y servicios de sistema. No está vinculado ni forma parte de la arquitectura de otros proyectos o repositorios presentados en este perfil.

---

**Características Principales**

* **Interfaz Oscura y Limpia:** Estructurada mediante layouts de `Grid` responsivos con paleta de colores personalizada.
* **Organización por Fechas:** Planificación de tareas para el día actual, vista rápida para el día siguiente e inspección de días anteriores.
* **Sección de Tareas Vencidas:** Panel dinámico para identificar rápidamente elementos pendientes de fechas pasadas.
* **Ejecución en Segundo Plano (System Tray):** Al presionar el botón de cierre ($X$), la aplicación se oculta en el área de notificación junto al reloj de Windows sin interrumpir el flujo de trabajo.
* **Inicio Automático con Windows:** Configuración opcional desde la interfaz mediante la manipulación del Registro de Windows (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
* **Persistencia Local:** Almacenamiento rápido en formato JSON sin necesidad de bases de datos externas.

---

**Tecnologías y Herramientas**

* **Lenguaje:** C# 12
* **Framework:** .NET 8.0 (WPF)
* **Integración de Sistema:** WinForms Interop (`System.Windows.Forms.NotifyIcon`) para la bandeja del sistema.
* **IDE:** Visual Studio 2022

---

Guía de Instalación y Ejecución

Si querés probar la aplicación en tu máquina local, podés hacerlo clonando el repositorio o descargando el código fuente.

### Requisitos Previos

* Tener instalado [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (o superior).
* Sistema Operativo **Windows 10 / 11**.
* *Opcional:* Visual Studio 2022 con la carga de trabajo *Desarrollo de escritorio de .NET*.

---

### Descarga Directa (.exe)

Si solo querés usar la aplicación sin compilar el código:
1. Andá a la sección de [Releases de este repositorio](https://github.com/gerogc/TaskPlanner/releases).
2. Descargá el archivo `TaskPlanner.exe` adjunto en la última versión.
3. Ejecutalo directamente en Windows 10/11.
