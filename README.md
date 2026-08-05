# Assignment & Submission Management System

A 3-tier REST API backend built with **ASP.NET Core** and **PostgreSQL**.

---

## 🗄️ Database Architecture (ERD)

Below is the Entity-Relationship Diagram (ERD) for the database schema:

```mermaid
erDiagram
  USERS ||--o{ TEACHER_SUBJECT_ASSIGNMENTS : teaches
  USERS |o--o{ TEACHER_SUBJECT_ASSIGNMENTS : assigns
  USERS ||--o{ STUDENT_ENROLLMENTS : enrolls
  USERS ||--o{ SUBMISSIONS : submits
  USERS |o--o{ SUBMISSIONS : grades
  USERS |o--o{ SUBMISSION_STATUS_HISTORY : changes
  USERS ||--o{ REFRESH_TOKENS : owns
  USERS |o--o{ APP_SETTINGS : updates
  CLASSES ||--o{ CLASS_SUBJECTS : includes
  SUBJECTS ||--o{ CLASS_SUBJECTS : includes
  CLASSES ||--o{ STUDENT_ENROLLMENTS : has
  CLASS_SUBJECTS ||--o{ TEACHER_SUBJECT_ASSIGNMENTS : covers
  TEACHER_SUBJECT_ASSIGNMENTS ||--o{ ASSIGNMENTS : creates
  ASSIGNMENTS ||--o{ ASSIGNMENT_ATTACHMENTS : has
  ASSIGNMENTS ||--o{ SUBMISSIONS : receives
  SUBMISSIONS ||--o{ SUBMISSION_ATTACHMENTS : has
  SUBMISSIONS ||--o{ SUBMISSION_STATUS_HISTORY : has

  USERS {
    uuid id PK
    string full_name
    string email
    string password_hash
    string role
    boolean is_active
    timestamp created_at
    timestamp updated_at
  }
  CLASSES {
    uuid id PK
    string name
    string academic_year
    boolean is_active
    timestamp created_at
  }
  SUBJECTS {
    uuid id PK
    string name
    string code
    boolean is_active
    timestamp created_at
  }
  CLASS_SUBJECTS {
    uuid id PK
    uuid class_id FK
    uuid subject_id FK
    timestamp created_at
  }
  TEACHER_SUBJECT_ASSIGNMENTS {
    uuid id PK
    uuid teacher_id FK
    uuid class_subject_id FK
    uuid assigned_by FK
    timestamp assigned_at
  }
  STUDENT_ENROLLMENTS {
    uuid id PK
    uuid student_id FK
    uuid class_id FK
    string roll_number
    string status
    timestamp enrolled_at
  }
  ASSIGNMENTS {
    uuid id PK
    uuid teacher_subject_assignment_id FK
    string title
    text description
    timestamp deadline
    numeric max_marks
    boolean allow_resubmission
    string status
    boolean is_deleted
    timestamp created_at
    timestamp updated_at
  }
  ASSIGNMENT_ATTACHMENTS {
    uuid id PK
    uuid assignment_id FK
    string file_name
    string file_url
    string content_type
    integer file_size_bytes
    timestamp uploaded_at
  }
  SUBMISSIONS {
    uuid id PK
    uuid assignment_id FK
    uuid student_id FK
    text answer_text
    timestamp submitted_at
    timestamp updated_at
    string status
    numeric marks_obtained
    text feedback
    uuid graded_by FK
    timestamp graded_at
  }
  SUBMISSION_ATTACHMENTS {
    uuid id PK
    uuid submission_id FK
    string file_name
    string file_url
    string content_type
    integer file_size_bytes
    timestamp uploaded_at
  }
  SUBMISSION_STATUS_HISTORY {
    uuid id PK
    uuid submission_id FK
    string old_status
    string new_status
    uuid changed_by FK
    timestamp changed_at
    string remarks
  }
  REFRESH_TOKENS {
    uuid id PK
    uuid user_id FK
    string token_hash
    timestamp expires_at
    timestamp created_at
    timestamp revoked_at
    string created_by_ip
  }
  APP_SETTINGS {
    string key PK
    text value
    uuid updated_by FK
    timestamp updated_at
  }
```

---

## Key Schema & Architectural Decisions

| Decision | Rationale |
| :--- | :--- |
| **Bridge Table (`class_subjects`)** | A subject like "Physics" is taught in multiple classes, and a class has multiple subjects. That's a many-to-many relationship, and the only clean way to store one is a bridge table.  |
| **Chained Hierarchy (`class_subjects` → `teacher_subject_assignments` → `assignments`)** | Enforces core domain integrity at the database level. Teachers can only be assigned to valid class-subject pairs, and assignments automatically inherit verified authorization rules. |
| **Unique Constraint on `(assignment_id, student_id)`** | Guarantees one submission per student per assignment, making resubmissions an atomic **insert or update** operation. |
| **Separate `refresh_tokens` entity** | A JWT access token should be short-lived (here defined as 15 minutes) for security. A refresh token, stored server-side is used to remember that user in server side so it can be revoked (e.g., on logout, or if compromised) — something a bare JWT can't do on its own since it's stateless. This table is what makes "logout" actually mean something. |
| **UUID Primary Keys** | With integers, `/api/submissions/14` tells anyone the id-guessing game is trivial — someone could iterate through every submission by changing one digit. UUIDs close that off. |
| **Soft Deletes (`is_deleted` / `is_active`)** | Preserves historical grading and submission data even if parent entities or assignments are removed. |
| **Dynamic `appsettings` Table** | Provides a key-value store for application-wide administrator configurations without requiring schema migrations. |
