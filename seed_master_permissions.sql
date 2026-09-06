-- ==============================================================================
-- Master Permissions & Default Role-Permission Mapping Seed Script
-- Database: Azure SQL / SQL Server (hrmdb)
-- Idempotent: Uses IF NOT EXISTS to prevent duplicate key errors
-- ==============================================================================

SET NOCOUNT ON;

DECLARE @OrgId INT = 1;
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

PRINT '>>> Seeding Master Permissions for Organization ID: ' + CAST(@OrgId AS VARCHAR(10));

-- Temporary table to hold permission definitions
CREATE TABLE #TempPermissions (
    Name NVARCHAR(150),
    Category NVARCHAR(100),
    Description NVARCHAR(500)
);

INSERT INTO #TempPermissions (Name, Category, Description) VALUES
-- Organization Masters
('Organization.View', 'Organization', 'View organization profile, settings, and hierarchy'),
('Organization.Manage', 'Organization', 'Update organization legal identity, currency, and fiscal settings'),
('Organization.Update', 'Organization', 'Update organization profile and branding logo'),
('Location.View', 'Organization', 'View campus locations and geofence coordinates'),
('Location.Manage', 'Organization', 'Create, update, and configure campus locations and geofences'),
('Location.Create', 'Organization', 'Create new campus locations'),
('Location.Edit', 'Organization', 'Edit campus locations'),
('Location.Delete', 'Organization', 'Delete or deactivate campus locations'),
('Location.CaptureGPS', 'Organization', 'Capture device GPS to automatically populate campus coordinates'),
('Department.View', 'Organization', 'View organizational departmental units'),
('Department.Manage', 'Organization', 'Create, update, and delete departments and head assignments'),
('Department.Create', 'Organization', 'Create new departmental units'),
('Department.Edit', 'Organization', 'Edit department details'),
('Department.Delete', 'Organization', 'Remove or deactivate departments'),
('Designation.View', 'Organization', 'View job designations and career bands'),
('Designation.Manage', 'Organization', 'Create, update, and delete job designations'),
('Designation.Create', 'Organization', 'Create new job designations'),
('Designation.Edit', 'Organization', 'Edit job designations'),
('Designation.Delete', 'Organization', 'Remove or deactivate job designations'),
('AcademicYear.View', 'Organization', 'View academic and fiscal operational cycles'),
('AcademicYear.Manage', 'Organization', 'Create, update, and activate academic cycles'),

-- Employee Directory
('Employee.View', 'Employees', 'View employee directory and organizational profiles'),
('Employee.Create', 'Employees', 'Onboard new employees with complete 33-field profile'),
('Employee.Edit', 'Employees', 'Edit administrative, salary, and job profile of any employee'),
('Employee.Update', 'Employees', 'Edit and update employee profile details and media'),
('Employee.Delete', 'Employees', 'Deactivate or terminate employee records'),
('Employee.Manage', 'Employees', 'Full employee directory administration and management'),
('Employee.ViewSelf', 'Employees', 'View own employee profile, documents, and compensation overview'),
('Employee.EditSelf', 'Employees', 'Self-service edit for personal contact details and emergency contacts'),

-- Attendance & WebClock
('Attendance.View', 'Attendance', 'View personal attendance logs and punches'),
('Attendance.ViewAll', 'Attendance', 'View organization-wide employee attendance and biometric punches'),
('Attendance.Punch', 'Attendance', 'Perform web clock-in and clock-out'),
('Attendance.ManualPunch', 'Attendance', 'Submit manual attendance punch adjustment requests'),
('Attendance.Manage', 'Attendance', 'Administratively edit and regularize attendance records'),
('Attendance.Regularize', 'Attendance', 'Submit and approve attendance regularization requests'),

-- Leaves & Policies
('Leave.Apply', 'Leaves', 'Submit leave applications and time-off requests'),
('Leave.View', 'Leaves', 'View own leave quota balances and application history'),
('Leave.ViewAll', 'Leaves', 'View all department and organization leave applications'),
('Leave.Approve', 'Leaves', 'Approve employee leave applications'),
('Leave.Reject', 'Leaves', 'Reject employee leave applications'),
('Leave.Cancel', 'Leaves', 'Cancel own or approved employee leave applications'),
('Leave.ManageTypes', 'Leaves', 'Configure leave categories, quotas, and accrual rules'),
('Leave.Configure', 'Leaves', 'Configure annual leave balances, policy entitlements, and carry forwards'),
('Leave.Adjust', 'Leaves', 'Manually adjust employee leave balances with audit history'),

-- Overtime Operations
('Overtime.View', 'Overtime', 'View overtime logs and compensatory multiplier policies'),
('Overtime.Request', 'Overtime', 'Request and submit overtime claims'),
('Overtime.Approve', 'Overtime', 'Approve employee overtime claims'),
('Overtime.Reject', 'Overtime', 'Reject employee overtime claims'),
('Overtime.Manage', 'Overtime', 'Configure overtime rules, daily caps, and rate multipliers'),

-- Shifts & Schedules
('Shift.View', 'Shifts', 'View shift templates and roster schedules'),
('Shift.Create', 'Shifts', 'Create custom work shifts and grace periods'),
('Shift.Edit', 'Shifts', 'Edit shift start/end times and break configurations'),
('Shift.Update', 'Shifts', 'Update shift details and roster assignments'),
('Shift.Delete', 'Shifts', 'Remove or deactivate shift templates'),
('Shift.Assign', 'Shifts', 'Assign work shifts to individual employees or entire departments'),

-- Holidays & OffDays
('Holiday.View', 'Holidays', 'View organization holidays and declared observances'),
('Holiday.Manage', 'Holidays', 'Create, edit, and delete organization and campus holidays'),
('OffDay.View', 'Holidays', 'View weekly off-day policies and weekend schedules'),
('OffDay.Manage', 'Holidays', 'Create and manage role-wise weekend policies and recurrence rules'),

-- Security & RBAC
('Role.View', 'Roles', 'View security roles and access privileges'),
('Role.Create', 'Roles', 'Create new security roles'),
('Role.Edit', 'Roles', 'Edit security role names and descriptions'),
('Role.Update', 'Roles', 'Update security role configurations'),
('Role.Delete', 'Roles', 'Remove custom security roles'),
('Role.Assign', 'Roles', 'Assign roles to employees'),
('Permission.View', 'Roles', 'View available permissions across modules'),
('Permission.Manage', 'Roles', 'Configure and assign permissions to system roles'),

-- Notifications
('Notification.View', 'Notifications', 'View personal system notifications and alerts'),
('Notification.Manage', 'Notifications', 'Broadcast organizational announcements and notifications');

-- Insert permissions into PermissionMaster if they do not exist
INSERT INTO [PermissionMaster] ([OrganizationId], [Name], [Category], [Description], [CreatedAt], [IsDeleted])
SELECT 
    @OrgId, 
    tp.Name, 
    tp.Category, 
    tp.Description, 
    @Now, 
    0
FROM #TempPermissions tp
WHERE NOT EXISTS (
    SELECT 1 
    FROM [PermissionMaster] pm 
    WHERE pm.[OrganizationId] = @OrgId AND pm.[Name] = tp.Name
);

DECLARE @PermCount INT;
SELECT @PermCount = COUNT(*) FROM [PermissionMaster] WHERE [OrganizationId] = @OrgId;
PRINT '>>> Inserted Master Permissions successfully. Total in database: ' + CAST(@PermCount AS VARCHAR(10));

-- ==============================================================================
-- Assign Default Permissions to System Roles
-- ==============================================================================

DECLARE @SuperAdminRoleId INT = (SELECT TOP 1 Id FROM [Roles] WHERE [OrganizationId] = @OrgId AND [Name] = 'Super Admin');
DECLARE @HrAdminRoleId INT = (SELECT TOP 1 Id FROM [Roles] WHERE [OrganizationId] = @OrgId AND [Name] = 'HR/Admin');
DECLARE @ManagerRoleId INT = (SELECT TOP 1 Id FROM [Roles] WHERE [OrganizationId] = @OrgId AND [Name] = 'Manager');
DECLARE @EmployeeRoleId INT = (SELECT TOP 1 Id FROM [Roles] WHERE [OrganizationId] = @OrgId AND [Name] = 'Employee');

-- 1. Super Admin: ALL Permissions
IF @SuperAdminRoleId IS NOT NULL
BEGIN
    INSERT INTO [RolePermissions] ([OrganizationId], [RoleId], [PermissionId], [CreatedAt], [IsDeleted])
    SELECT @OrgId, @SuperAdminRoleId, pm.Id, @Now, 0
    FROM [PermissionMaster] pm
    WHERE pm.[OrganizationId] = @OrgId
      AND NOT EXISTS (
          SELECT 1 FROM [RolePermissions] rp 
          WHERE rp.[OrganizationId] = @OrgId AND rp.[RoleId] = @SuperAdminRoleId AND rp.[PermissionId] = pm.Id
      );
    PRINT '>>> Assigned all permissions to Super Admin role.';
END

-- 2. HR/Admin: ALL Permissions
IF @HrAdminRoleId IS NOT NULL
BEGIN
    INSERT INTO [RolePermissions] ([OrganizationId], [RoleId], [PermissionId], [CreatedAt], [IsDeleted])
    SELECT @OrgId, @HrAdminRoleId, pm.Id, @Now, 0
    FROM [PermissionMaster] pm
    WHERE pm.[OrganizationId] = @OrgId
      AND NOT EXISTS (
          SELECT 1 FROM [RolePermissions] rp 
          WHERE rp.[OrganizationId] = @OrgId AND rp.[RoleId] = @HrAdminRoleId AND rp.[PermissionId] = pm.Id
      );
    PRINT '>>> Assigned all permissions to HR/Admin role.';
END

-- 3. Manager Permissions
IF @ManagerRoleId IS NOT NULL
BEGIN
    INSERT INTO [RolePermissions] ([OrganizationId], [RoleId], [PermissionId], [CreatedAt], [IsDeleted])
    SELECT @OrgId, @ManagerRoleId, pm.Id, @Now, 0
    FROM [PermissionMaster] pm
    WHERE pm.[OrganizationId] = @OrgId
      AND pm.[Name] IN (
          'Organization.View', 'Location.View', 'Department.View', 'Designation.View', 'AcademicYear.View',
          'Employee.View', 'Employee.Update', 'Employee.Edit', 'Employee.ViewSelf', 'Employee.EditSelf',
          'Attendance.View', 'Attendance.ViewAll', 'Attendance.Punch', 'Attendance.ManualPunch', 'Attendance.Regularize',
          'Leave.Apply', 'Leave.View', 'Leave.ViewAll', 'Leave.Approve', 'Leave.Reject', 'Leave.Cancel',
          'Overtime.View', 'Overtime.Request', 'Overtime.Approve', 'Overtime.Reject',
          'Shift.View', 'Shift.Assign',
          'Holiday.View', 'OffDay.View',
          'Notification.View'
      )
      AND NOT EXISTS (
          SELECT 1 FROM [RolePermissions] rp 
          WHERE rp.[OrganizationId] = @OrgId AND rp.[RoleId] = @ManagerRoleId AND rp.[PermissionId] = pm.Id
      );
    PRINT '>>> Assigned manager permissions to Manager role.';
END

-- 4. Employee Permissions
IF @EmployeeRoleId IS NOT NULL
BEGIN
    INSERT INTO [RolePermissions] ([OrganizationId], [RoleId], [PermissionId], [CreatedAt], [IsDeleted])
    SELECT @OrgId, @EmployeeRoleId, pm.Id, @Now, 0
    FROM [PermissionMaster] pm
    WHERE pm.[OrganizationId] = @OrgId
      AND pm.[Name] IN (
          'Employee.ViewSelf', 'Employee.EditSelf',
          'Attendance.View', 'Attendance.Punch', 'Attendance.ManualPunch',
          'Leave.Apply', 'Leave.View',
          'Overtime.View', 'Overtime.Request',
          'Shift.View',
          'Holiday.View', 'OffDay.View',
          'Notification.View'
      )
      AND NOT EXISTS (
          SELECT 1 FROM [RolePermissions] rp 
          WHERE rp.[OrganizationId] = @OrgId AND rp.[RoleId] = @EmployeeRoleId AND rp.[PermissionId] = pm.Id
      );
    PRINT '>>> Assigned employee self-service permissions to Employee role.';
END

DROP TABLE #TempPermissions;
PRINT '>>> Master Permissions Seeding Completed Successfully.';
