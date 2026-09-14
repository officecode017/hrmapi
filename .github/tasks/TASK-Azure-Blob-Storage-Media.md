# Task: Migrate Profile Photos, Cover Photos, and Organization Logos to Azure Blob Storage

**Type**: Feature / Refactor  
**Priority**: High  
**Labels**: `backend`, `cloud-storage`, `azure`, `enhancement`  
**Target Milestone**: Cloud Media Persistence & Multi-instance Scalability  

---

## 1. Context & Motivation

Currently, employee profile pictures (`Employee.PhotoPath`), cover photos (`Employee.BackgroundImagePath`), and organization logos (`Organization.LogoUrl`) are saved directly to the server's local file system inside the `/uploads` directory using `LocalFileStorageService`.

### Problem Statement:
- **Ephemeral Storage in Cloud/App Services**: In cloud hosting environments like Azure App Service, Azure Container Apps, or Kubernetes, the local web server filesystem is ephemeral. Deployments, restarts, and horizontal scaling cause uploaded profile images, cover pictures, and company logos to be lost or desynchronized across server instances.
- **Existing Azure Blob Infrastructure**: With the recent introduction of `IAzureBlobStorageService` and `AzureBlobStorageService` for employee document management, the foundational Azure Blob Storage connection and client SDK are already integrated into the solution.
- **Goal**: Consolidate all uploaded image assets (user avatars, user cover photos, organization logos) into Azure Blob Storage to achieve persistent, scalable, and CDN-ready cloud storage.

---

## 2. Scope & Requirements

### A. Employee Profile Photos
- **Entity Property**: `HRAttendance.Data.Models.Employee.Employee.PhotoPath`
- **Endpoints**:
  - `POST /api/employees/{id}/photo` (Upload & update photo)
  - `DELETE /api/employees/{id}/photo` (Remove photo)
- **Target Blob Prefix**: `media/photos/{organizationId}/{employeeId}_{guid}.{ext}` (or dedicated container `hrm-media`)
- **Requirement**: The uploaded photo must be stored as a blob in Azure Blob Storage, and the saved URL stored in `Employee.PhotoPath` must be the persistent Azure Blob URL. When replacing or deleting a photo, the existing blob must be deleted from Azure Storage.

### B. Employee Cover / Background Photos
- **Entity Property**: `HRAttendance.Data.Models.Employee.Employee.BackgroundImagePath`
- **Endpoints**:
  - `POST /api/employees/{id}/background` (Upload & update cover photo)
  - `DELETE /api/employees/{id}/background` (Remove cover photo)
- **Target Blob Prefix**: `media/backgrounds/{organizationId}/{employeeId}_{guid}.{ext}`
- **Requirement**: Cover images must be stored in Azure Blob Storage, and the existing blob must be deleted when updated or removed.

### C. Organization Logos
- **Entity Property**: `HRAttendance.Data.Models.Organization.Organization.LogoUrl`
- **Endpoints**:
  - `POST /api/organizations/{id}/logo` (Upload & update organization logo)
  - `DELETE /api/organizations/{id}/logo` (Remove organization logo)
- **Target Blob Prefix**: `media/logos/{organizationId}/logo_{guid}.{ext}`
- **Requirement**: Logos must be uploaded to Azure Blob Storage. Old logo blobs must be removed upon update or deletion.

### D. Payslip PDF Generator Integration
- **Service**: `HRAttendance.Business.Services.Payroll.QuestPdfGenerator` (`LoadCompanyLogo`)
- **Requirement**: Ensure `LoadCompanyLogo` can fetch and embed company logos stored as Azure Blob URLs when rendering payslip PDFs. (It already has HTTP URL fetch logic; verify timeout, caching, and stream handling).

### E. Frontend Compatibility
- **Component**: `frontend/src/lib/media.ts` (`getImageUrl`)
- **Requirement**: `getImageUrl` already detects and returns absolute URLs (`http://`, `https://`). Confirm that no frontend regression occurs and that images render properly across `Sidebar`, `Header`, `EmployeeProfilePage`, `EmployeeDirectoryPage`, and `OrganizationMastersPage`.

---

## 3. Technical Architecture & Implementation Plan

### 3.1. Storage Provider Strategy
Choose between one of two approaches:
- **Approach 1 (Recommended - Interface Polymorphism)**:
  - Create `AzureBlobFileStorageService : IFileStorageService` (or make `FileStorageService` delegate to `IAzureBlobStorageService`).
  - Swap `services.AddScoped<IFileStorageService, AzureBlobFileStorageService>()` in `DependencyInjectionExtensions.cs`.
  - Maintain the local fallback logic from `AzureBlobStorageService` when running offline or without connection credentials.
- **Approach 2 (Direct Service Delegation)**:
  - Inject `IAzureBlobStorageService` directly into `EmployeeService` and `OrganizationService`.

### 3.2. Blob Security & Container Access
- Since profile pictures, cover images, and company logos are displayed publicly/within authenticated web sessions:
  - Use a container configured for Blob Public Read access (e.g. `PublicAccessType.Blob`), OR
  - Generate SAS tokens with reasonable expiration, OR
  - Serve via an API streaming proxy `/api/media/{blobName}` if assets require tenant-isolated permission enforcement.
  - *Recommendation*: Use a dedicated public-read container (e.g., `hrm-public-assets`) or blob-level public access for avatars, banners, and logos.

### 3.3. Backward Compatibility & Fallback
- Support existing `/uploads/...` paths during the transition period:
  - When deleting old files, check if path starts with `https://` (delete via Azure Blob Storage) or `/uploads/` (delete via local disk fallback).
  - Add an optional migration utility/script to migrate legacy disk images in `/uploads` to Azure Blob Storage if needed.

---

## 4. Tasks & Checklist

- [ ] **Backend Service Layer**:
  - [ ] Implement `AzureBlobFileStorageService` (or adapt `IFileStorageService` to use `IAzureBlobStorageService`).
  - [ ] Configure container/blob path naming convention (`photos`, `backgrounds`, `logos`).
  - [ ] Update `EmployeeService.UpdatePhotoAsync` and `RemovePhotoAsync` to handle Azure blobs.
  - [ ] Update `EmployeeService.UpdateBackgroundAsync` and `RemoveBackgroundAsync` to handle Azure blobs.
  - [ ] Update `OrganizationService.UpdateLogoAsync` and `RemoveLogoAsync` to handle Azure blobs.
- [ ] **Configuration & Dependency Injection**:
  - [ ] Update `DependencyInjectionExtensions.cs` to bind the Azure storage service for media uploads.
  - [ ] Add container settings in `appsettings.json` and `appsettings.Local.json` (e.g., `AzureBlobStorage:MediaContainerName`).
- [ ] **Payslip PDF Integration**:
  - [ ] Verify `QuestPdfGenerator.LoadCompanyLogo` downloads the Azure Blob logo stream reliably.
  - [ ] Add unit/integration tests for logo loading from Azure Blob URLs.
- [ ] **Frontend Verification**:
  - [ ] Verify avatar rendering in `Header.tsx` and `Sidebar.tsx`.
  - [ ] Verify cover photo and avatar in `EmployeeProfilePage.tsx`.
  - [ ] Verify logo in `LoginPage.tsx` and `OrganizationMastersPage.tsx`.
- [ ] **Automated Tests**:
  - [ ] Unit tests for `EmployeeService` photo & background operations with mocked `IAzureBlobStorageService`.
  - [ ] Unit tests for `OrganizationService` logo operations with mocked `IAzureBlobStorageService`.

---

## 5. Acceptance Criteria

1. **Uploads Persist in Azure**: Uploading an employee profile photo, background banner, or organization logo saves the file to Azure Blob Storage and stores the Azure Blob URL in the database.
2. **Old Blobs Cleaned Up**: Uploading a new image or removing an image deletes the previous blob from Azure Storage to prevent orphan files and storage bloat.
3. **No Local Disk Dependency**: Running the backend in an environment without persistent local disk (e.g., container / Azure App Service) allows image uploads and views to function without data loss.
4. **Offline / Fallback Support**: When Azure connection string is absent or in offline dev mode, the service falls back gracefully without unhandled exceptions.
5. **Payslips Render with Logo**: Generated payslip PDFs successfully render the company logo sourced from Azure Blob Storage.
6. **Frontend Compatibility**: Avatars, cover photos, and logos display without broken image links across all existing views.

---

## 6. Key Code References

- **Backend**:
  - Entity: `src/HRAttendance.Data/Models/Employee/Employee.cs` (`PhotoPath`, `BackgroundImagePath`)
  - Entity: `src/HRAttendance.Data/Models/Organization/Organization.cs` (`LogoUrl`)
  - Current File Storage: `src/HRAttendance.Business/Services/LocalFileStorageService.cs`
  - Azure Blob Service: `src/HRAttendance.Business/Services/Documents/AzureBlobStorageService.cs`
  - Employee Service: `src/HRAttendance.Business/Services/EmployeeService.cs`
  - Organization Service: `src/HRAttendance.Business/Services/OrganizationService.cs`
  - PDF Payslip Generator: `src/HRAttendance.Business/Services/Payroll/QuestPdfGenerator.cs`
  - DI Registration: `src/HRAttendance.API/Extensions/DependencyInjectionExtensions.cs`
- **Frontend**:
  - Media Resolver: `frontend/src/lib/media.ts` (`getImageUrl`)
  - Profile & Cover: `frontend/src/features/employee/EmployeeProfilePage.tsx`
  - Directory: `frontend/src/features/employee/EmployeeDirectoryPage.tsx`
  - Layout: `frontend/src/app/layout/Header.tsx`, `Sidebar.tsx`
