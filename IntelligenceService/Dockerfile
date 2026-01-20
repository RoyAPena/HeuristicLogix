# Usamos una base estable y ligera
FROM python:3.11-slim-bookworm

# Evitar prompts de terminal durante la instalación
ENV DEBIAN_FRONTEND=noninteractive

WORKDIR /app

# Instalación moderna de dependencias y drivers de SQL Server
RUN apt-get update && apt-get install -y \
    curl \
    ca-certificates \
    gnupg2 \
    unixodbc \
    unixodbc-dev \
    && mkdir -p /etc/apt/keyrings \
    && curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/keyrings/microsoft.gpg \
    && echo "deb [arch=amd64,arm64,armhf signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql18 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copiar requerimientos e instalar (Caché de capas de Docker)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copiar el resto del código
COPY . .

# Exponer el puerto de FastAPI
EXPOSE 8000

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]