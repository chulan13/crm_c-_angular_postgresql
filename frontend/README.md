# CRM frontend

Angular 19 standalone application for managing CRM tasks. The interface uses native HTML controls and custom CSS (no UI component library) and Angular `HttpClient` to call the ASP.NET API. It adapts to mobile screens, where task rows are displayed as stacked cards.

## Run locally

Start the ASP.NET API from the repository root:

```bash
dotnet run
```

In another terminal, start Angular:

```bash
npm start --prefix frontend
```

Open <http://localhost:4200>. The Angular development proxy in `proxy.conf.json` forwards `/CrmTask`, `/Department`, and `/Status` requests to the API at `http://localhost:5283`, avoiding browser CORS configuration during local development.

If the API uses another port, update the proxy targets. Production deployments should configure the API and frontend under the same origin or provide an appropriate API base URL and CORS policy.

## Build

```bash
npm run build --prefix frontend
```
