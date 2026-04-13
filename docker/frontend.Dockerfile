FROM node:18-alpine
WORKDIR /app
COPY frontend/package*.json ./frontend/
RUN cd frontend && npm ci
COPY frontend .
WORKDIR /app
CMD ["npm", "run", "dev", "--prefix", "frontend"]
