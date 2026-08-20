# HelpDesk Lite — Demo Script

## Introduction

HelpDesk Lite is an internal support ticketing workspace designed to replace scattered support requests across email, chat, and informal follow-up with one structured workflow.

The main workflow is:

**Submit → Assign → Handle → Resolve → Track**

The system supports three roles: Employee, Support Agent, and Manager.

## Employee Flow

I’ll start from the Employee experience.

An Employee can create a structured support ticket by entering the request details and submitting it.

After submission, the ticket appears in **My Tickets**, where the Employee can open the ticket details and track its current status, assigned Support Agent, progress updates, and attachments.

This gives the Employee one clear place to follow the request instead of repeatedly asking for updates through chat or email.

## Support Agent Flow

Next, I’ll switch to the Support Agent experience.

The Support Agent can open the **Support Queue**, find the new request, assign or take ownership of it, update its status, and add progress updates.

The ticket can move through statuses such as:

**Open → In Progress → In Review → Resolved**

These changes are persisted and visible in the ticket activity history.

## Manager Flow

Finally, the Manager dashboard provides read-only operational visibility.

Managers can review ticket counts, unassigned work, active workload, recent tickets, and tickets that need attention.

The Manager can inspect ticket details but cannot modify the workflow.

## Proof It Works

HelpDesk Lite uses React and TypeScript on the frontend, ASP.NET Core Web API and Identity on the backend, and Entity Framework Core with SQL Server for persistence.

The project has automated backend and authentication tests, frontend lint/build verification, and it is deployed and tested in production.

**Live Demo:**  
https://helpdesklite.runasp.net

The result is a lightweight end-to-end support workflow with clear ownership, progress tracking, role-based access, and manager visibility.
