import { useState, useEffect } from 'react';
import api from '../api/axios';

interface Application {
  id: number;
  jobId: number;
  status: string;
  appliedDate: string;
  job: { title: string; companyName: string; };
}

export default function MyApplications() {
  const [apps, setApps] = useState<Application[]>([]);

  useEffect(() => {
    api.get('/applications/my').then((res) => setApps(res.data));
  }, []);

  return (
    <div className="container">
      <h1>My Applications</h1>
      {apps.length === 0 ? (
        <p>You haven't applied to any jobs yet.</p>
      ) : (
        <div className="applications-list">
          {apps.map((app) => (
            <div key={app.id} className="application-card">
              <div className="app-info">
                <h3>{app.job.title}</h3>
                <p className="company">{app.job.companyName}</p>
                <p className="date">Applied: {new Date(app.appliedDate).toLocaleDateString()}</p>
              </div>
              <span className={`badge status-${app.status.toLowerCase()}`}>{app.status}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
