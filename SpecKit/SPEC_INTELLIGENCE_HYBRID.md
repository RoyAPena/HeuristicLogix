\# Hybrid Intelligence Architecture - HeuristicLogix 2026



\## 1. Local Tier: Edge Intelligence (FastAPI + ML.NET)

\- \*\*Scope:\*\* Logistics \& POS.

\- \*\*Latency:\*\* <50ms.

\- \*\*Tasks:\*\* - Real-time truck capacity scoring (Logistics).

&nbsp;   - Anomaly detection in Cash Register transactions (POS).

&nbsp;   - Instant product recommendations based on local stock (Inventory).



\## 2. Cloud Tier: Contextual Validation (Gemini 2.5 Flash)

\- \*\*Trigger:\*\* Every Kafka Event from any module.

\- \*\*Tasks:\*\* - \*\*Logistics:\*\* Labeling expert overrides (e.g., "Dad changed truck because of rain").

&nbsp;   - \*\*Finance:\*\* Risk assessment for credit requests above the limit.

&nbsp;   - \*\*Inventory:\*\* Tagging high-shrinkage items (merma).



\## 3. Cloud Tier: Strategic Intelligence (GPT-5.2)

\- \*\*Frequency:\*\* Weekly / Monthly Batches.

\- \*\*Source:\*\* Combined SQL History (Finance + Inventory + Logistics).

\- \*\*Tasks:\*\* - \*\*Procurement:\*\* Analyzing supplier lead times vs. seasonality to optimize buying.

&nbsp;   - \*\*Finance:\*\* Predictive Cash Flow analysis for the next 30 days.

&nbsp;   - \*\*Strategic:\*\* "Expert Discovery Report" - Summarizing the wisdom captured from the father's overrides.

