import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./index.css";

import { GoogleOAuthProvider } from "@react-oauth/google";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <GoogleOAuthProvider clientId="108910087816-svtkr5ejnjjsom3cq9jss8n5fbpcrle5.apps.googleusercontent.com">
      <App />
    </GoogleOAuthProvider>
  </React.StrictMode>,
);
