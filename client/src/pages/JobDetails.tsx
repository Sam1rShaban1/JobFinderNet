import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

interface Job {
  id: number;
  title: string;
  description: string;
  companyName: string;
  location: string;
  jobType: string;
  salary: string;
  experienceRequired: string;
  postedDate: string;
  isActive: boolean;
}

export default function JobDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [job, setJob] = useState<Job | null>(null);
  const [message, setMessage] = useState('');

  useEffect(() => {
    api.get(`/jobs/${id}`).then((res) => setJob(res.data));
  }, [id]);

  const handleApply = async () => {
    try {
      const res = await api.post(`/applications/${id}`);
      setMessage(res.data.message || 'Application submitted!');
    } catch (err: any) {
      setMessage(err.response?.data?.message || 'Application failed');
    }
  };

  if (!job) return <div className="container"><p>Loading...</p></div>;

  return (
    <div className="container">
      <button onClick={() => navigate(-1)} className="btn btn-text">← Back</button>
      <div className="job-detail">
        <div className="detail-header">
          <h1>{job.title}</h1>
          <span className={`badge ${job.jobType.toLowerCase()}`}>{job.jobType}</span>
        </div>
        <div className="detail-meta">
          <div><strong>Company:</strong> {job.companyName}</div>
          <div><strong>Location:</strong> {job.location}</div>
          <div><strong>Salary:</strong> {job.salary}</div>
          <div><strong>Experience:</strong> {job.experienceRequired}</div>
          <div><strong>Posted:</strong> {new Date(job.postedDate).toLocaleDateString()}</div>
        </div>
        <div className="detail-description">
          <h3>Description</h3>
          <p>{job.description}</p>
        </div>
        {message && <div className={`alert ${message.includes('failed') || message.includes('already') ? 'alert-error' : 'alert-success'}`}>{message}</div>}
        {user?.role === 'Applicant' && job.isActive && (
          <button onClick={handleApply} className="btn btn-primary">Apply Now</button>
        )}
      </div>
    </div>
  );
}
