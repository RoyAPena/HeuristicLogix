\# SPEC\_DATA\_INGESTION: Ingesta de Datos MVP



\## 1. Estrategia de Carga

\- \*\*Bulk Load:\*\* Importación vía Excel (.xlsx / .csv) para datos históricos de los últimos 6 meses.

\- \*\*Fast Entry:\*\* Formulario Blazor optimizado para teclado (sin usar mouse) para la operación diaria.



\## 2. Contrato de Datos de Entrada (Excel/Form)

\- \*\*Fecha:\*\* DateTime (ISO).

\- \*\*ClienteId:\*\* String (Relacionado con Finance).

\- \*\*Items:\*\* Lista de productos y cantidades.

\- \*\*CamionAsignado:\*\* ID del camión usado.

\- \*\*DecisionExperto:\*\* Texto libre (El "Por qué" de tu padre).



\## 3. Flujo de Procesamiento

1\. El Backend valida la estructura del Excel.

2\. Cada fila se convierte en un `ConduceCreatedEvent` en el Outbox.

3\. Kafka distribuye los eventos.

4\. El servicio de Python (Gemini) recibe los eventos en "batch" y genera etiquetas de patrones de decisión.

