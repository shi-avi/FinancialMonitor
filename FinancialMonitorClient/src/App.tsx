import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import AddPage from "./pages/AddPage";
import MonitorPage from "./pages/MonitorPage";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/add" replace />} />
        <Route path="/add" element={<AddPage />} />
        <Route path="/monitor" element={<MonitorPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;