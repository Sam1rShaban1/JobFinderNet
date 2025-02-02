using Bogus;
using JobFinderNet.Models;
using JobFinderNet.Data;
using System;
using System.Collections.Generic;

namespace JobFinderNet.Factories;

public static class ApplicationFactory
{
    public static Faker<Application> Create(List<Job> jobs, List<ApplicationUser> applicants)
    {
        return new Faker<Application>()
            .RuleFor(a => a.AppliedDate, f => DateTime.SpecifyKind(f.Date.Past(1), DateTimeKind.Utc))
            .RuleFor(a => a.Status, f => ApplicationStatus.Pending)
            .RuleFor(a => a.JobId, (f, a) => f.PickRandom(jobs).Id)
            .RuleFor(a => a.Job, (f, a) => jobs.First(j => j.Id == a.JobId))
            .RuleFor(a => a.ApplicantId, f => f.PickRandom(applicants).Id)
            .RuleFor(a => a.Applicant, (f, a) => applicants.First(u => u.Id == a.ApplicantId))
            .RuleFor(a => a.CoverLetter, f => f.Lorem.Paragraphs(2))
            .RuleFor(a => a.ResumeUrl, f => f.Internet.Url());
    }
} 