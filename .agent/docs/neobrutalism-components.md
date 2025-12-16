# Neobrutalism UI Components Reference

Referencia rápida de componentes de [neobrutalism.dev](https://www.neobrutalism.dev/) para diseño en Parhelion WMS.

## Instalación Base

```bash
# Usar con shadcn CLI
pnpm dlx shadcn@latest add https://neobrutalism.dev/r/[component].json
```

---

## Componentes por Categoría

### 🎨 Básicos

| Componente | Descripción                                                | Instalación   |
| ---------- | ---------------------------------------------------------- | ------------- |
| **Button** | Botones con variantes: default, reverse, noShadow, neutral | `button.json` |
| **Badge**  | Etiquetas: default, neutral, withIcon                      | `badge.json`  |
| **Card**   | Contenedores con CardHeader, CardContent, CardFooter       | `card.json`   |
| **Avatar** | Imágenes de perfil circulares                              | `avatar.json` |

### 📝 Formularios

| Componente      | Descripción                  | Instalación        |
| --------------- | ---------------------------- | ------------------ |
| **Input**       | Campos de texto              | `input.json`       |
| **Checkbox**    | Casillas de verificación     | `checkbox.json`    |
| **Switch**      | Toggle on/off                | `switch.json`      |
| **Select**      | Dropdown de opciones         | `select.json`      |
| **Slider**      | Control deslizante           | `slider.json`      |
| **Radio Group** | Grupo de opciones exclusivas | `radio-group.json` |
| **Label**       | Etiquetas para inputs        | `label.json`       |
| **Textarea**    | Área de texto multilínea     | `textarea.json`    |

### 📊 Datos

| Componente     | Descripción                                 | Instalación       |
| -------------- | ------------------------------------------- | ----------------- |
| **Table**      | Tablas con TableHeader, TableRow, TableCell | `table.json`      |
| **Data Table** | Tablas avanzadas con sorting/filtering      | `data-table.json` |
| **Progress**   | Barras de progreso                          | `progress.json`   |
| **Chart**      | Gráficos (basado en Recharts)               | `chart.json`      |

### 🧭 Navegación

| Componente          | Descripción                                     | Instalación            |
| ------------------- | ----------------------------------------------- | ---------------------- |
| **Tabs**            | Pestañas con TabsList, TabsTrigger, TabsContent | `tabs.json`            |
| **Breadcrumb**      | Migas de pan                                    | `breadcrumb.json`      |
| **Navigation Menu** | Menú de navegación                              | `navigation-menu.json` |
| **Menubar**         | Barra de menú                                   | `menubar.json`         |
| **Sidebar**         | Barra lateral                                   | `sidebar.json`         |
| **Pagination**      | Paginación                                      | `pagination.json`      |

### 💬 Overlays

| Componente        | Descripción                     | Instalación          |
| ----------------- | ------------------------------- | -------------------- |
| **Dialog**        | Modales                         | `dialog.json`        |
| **Alert Dialog**  | Diálogos de confirmación        | `alert-dialog.json`  |
| **Sheet**         | Paneles deslizantes             | `sheet.json`         |
| **Drawer**        | Cajón desde abajo (mobile)      | `drawer.json`        |
| **Popover**       | Popups contextuales             | `popover.json`       |
| **Dropdown Menu** | Menús desplegables              | `dropdown-menu.json` |
| **Context Menu**  | Menú contextual (click derecho) | `context-menu.json`  |
| **Hover Card**    | Tarjeta al hover                | `hover-card.json`    |
| **Tooltip**       | Tooltips                        | `tooltip.json`       |

### 🎭 Especiales

| Componente      | Descripción                   | Instalación        |
| --------------- | ----------------------------- | ------------------ |
| **Accordion**   | Secciones colapsables         | `accordion.json`   |
| **Collapsible** | Contenido colapsable          | `collapsible.json` |
| **Carousel**    | Carrusel de imágenes          | `carousel.json`    |
| **Marquee**     | Texto en movimiento           | `marquee.json`     |
| **Image Card**  | Tarjeta con imagen            | `image-card.json`  |
| **Calendar**    | Selector de fecha             | `calendar.json`    |
| **Date Picker** | Picker de fechas              | `date-picker.json` |
| **Sonner**      | Notificaciones toast          | `sonner.json`      |
| **Alert**       | Mensajes de alerta            | `alert.json`       |
| **Skeleton**    | Placeholders de carga         | `skeleton.json`    |
| **Scroll Area** | Área con scroll personalizado | `scroll-area.json` |
| **Resizable**   | Paneles redimensionables      | `resizable.json`   |
| **Command**     | Command palette (⌘K)          | `command.json`     |
| **Combobox**    | Input con autocompletado      | `combobox.json`    |

---

## Ejemplos de Uso

### Button

```jsx
import { Button } from '@/components/ui/button'

<Button>Default</Button>
<Button variant="neutral">Neutral</Button>
<Button variant="reverse">Reverse</Button>
<Button variant="noShadow">No Shadow</Button>
```

### Card

```jsx
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from "@/components/ui/card";

<Card className="w-full max-w-sm">
  <CardHeader>
    <CardTitle>Título</CardTitle>
    <CardDescription>Descripción</CardDescription>
  </CardHeader>
  <CardContent>Contenido aquí</CardContent>
  <CardFooter>
    <Button>Acción</Button>
  </CardFooter>
</Card>;
```

### Tabs

```jsx
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";

<Tabs defaultValue="tab1">
  <TabsList>
    <TabsTrigger value="tab1">Tab 1</TabsTrigger>
    <TabsTrigger value="tab2">Tab 2</TabsTrigger>
  </TabsList>
  <TabsContent value="tab1">Contenido 1</TabsContent>
  <TabsContent value="tab2">Contenido 2</TabsContent>
</Tabs>;
```

### Progress

```jsx
import { Progress } from "@/components/ui/progress";

<Progress value={66} className="w-[60%]" />;
```

### Table

```jsx
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from "@/components/ui/table";

<Table>
  <TableHeader>
    <TableRow>
      <TableHead>Columna 1</TableHead>
      <TableHead>Columna 2</TableHead>
    </TableRow>
  </TableHeader>
  <TableBody>
    <TableRow>
      <TableCell>Dato 1</TableCell>
      <TableCell>Dato 2</TableCell>
    </TableRow>
  </TableBody>
</Table>;
```

### Switch

```jsx
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";

<div className="flex items-center space-x-2">
  <Switch id="modo" />
  <Label htmlFor="modo">Activar modo</Label>
</div>;
```

---

## Características del Estilo

- **Bordes gruesos**: 2px sólidos, típicamente negros
- **Sombras offset**: Box-shadow desplazadas (4px 4px 0)
- **Colores vivos**: Paletas llamativas, no sutiles
- **Sin bordes redondeados**: Esquinas rectas o mínimamente redondeadas
- **Alto contraste**: Texto legible sobre fondos brillantes
- **Hover states**: Transformaciones y cambios de color prominentes

## Links

- 📘 [Documentación oficial](https://www.neobrutalism.dev/docs)
- 🎨 [Estilos](https://www.neobrutalism.dev/styling)
- 📊 [Charts](https://www.neobrutalism.dev/charts)
- 🖼️ [Figma](https://www.neobrutalism.dev/docs/figma)
- 💻 [GitHub](https://github.com/ekmas/neobrutalism-components)
