/**
 * HRAttendance - Comprehensive End-to-End API Test Suite (Node.js ESM)
 * -------------------------------------------------------------------
 * Tests all endpoints in exact dependency order and captures:
 * - Endpoint
 * - Method
 * - Request header
 * - Payload
 * - Response
 * 
 * Saves results to:
 * 1. api_test_log.txt (human-readable log)
 * 2. api_test_results.json (structured JSON catalog for UI development)
 */

import fs from 'fs';

const BASE_URL = 'http://localhost:5050';
const RESULTS = [];
const LOG_LINES = [];

function log(text = '') {
  console.log(text);
  LOG_LINES.push(text);
}

function recordCall(stepNo, title, method, endpoint, headers, payload, statusCode, responseBody) {
  RESULTS.push({
    step: stepNo,
    title,
    method,
    endpoint,
    request_headers: headers,
    payload,
    status_code: statusCode,
    response: responseBody
  });

  log('='.repeat(80));
  log(`Step ${stepNo}: ${title}`);
  log(`Endpoint: ${endpoint}`);
  log(`Method: ${method}`);
  log(`Request header : ${JSON.stringify(headers, null, 2)}`);
  log(payload !== null ? `Payload:\n${JSON.stringify(payload, null, 2)}` : 'Payload: None');
  log(`Response: (HTTP ${statusCode})\n${typeof responseBody === 'object' ? JSON.stringify(responseBody, null, 2) : responseBody}`);
  log('='.repeat(80));
  log();
}

async function makeRequest(method, path, headers = {}, payload = null, stepNo = 1, title = '') {
  const url = `${BASE_URL}${path}`;
  const reqHeaders = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    ...headers
  };

  const displayHeaders = { ...reqHeaders };
  if (displayHeaders.Authorization && displayHeaders.Authorization.length > 30) {
    displayHeaders.Authorization = displayHeaders.Authorization.slice(0, 25) + '... [JWT TOKEN]';
  }

  let statusCode = 0;
  let responseData = null;

  try {
    const fetchOptions = {
      method,
      headers: reqHeaders
    };
    if (payload !== null && method !== 'GET' && method !== 'HEAD') {
      fetchOptions.body = JSON.stringify(payload);
    }

    const res = await fetch(url, fetchOptions);
    statusCode = res.status;
    const text = await res.text();
    try {
      responseData = text ? JSON.parse(text) : {};
    } catch {
      responseData = text;
    }
  } catch (err) {
    statusCode = 500;
    responseData = { error: err.message };
  }

  recordCall(stepNo, title, method, url, displayHeaders, payload, statusCode, responseData);
  return { status: statusCode, data: responseData };
}

async function runSuite() {
  log('='.repeat(80));
  log(`Starting HRAttendance API Test Suite at ${new Date().toISOString()}`);
  log(`Target API Server: ${BASE_URL}`);
  log('='.repeat(80) + '\n');

  let step = 1;

  // 1. Authentication
  const loginPayload = {
    organizationId: 1,
    employeeCodeOrEmail: 'admin@hrm.local',
    password: 'Password@123'
  };
  const loginRes = await makeRequest('POST', '/api/auth/login', {}, loginPayload, step++, 'Login as Admin to acquire JWT Bearer Token');
  const token = loginRes.data?.data?.token || '';
  const authHeader = token ? { Authorization: `Bearer ${token}` } : {};

  // 2. Organization Profile
  await makeRequest('GET', '/api/organization/1', authHeader, null, step++, 'Get Organization Details');
  await makeRequest('PUT', '/api/organization/1', authHeader, {
    name: 'Acme Global HR Solutions',
    phone: '+1-800-555-0199',
    email: 'corporate@acmeglobal.com',
    website: 'https://www.acmeglobal.com',
    address: '100 Enterprise Boulevard, Suite 500',
    city: 'Austin',
    state: 'Texas',
    country: 'USA',
    postalCode: '78701',
    industry: 'Information Technology',
    taxId: 'US-TAX-88992211',
    logoPath: 'https://assets.acmeglobal.com/logo.png',
    currency: 'USD',
    fiscalYearStartMonth: 1
  }, step++, 'Update Organization Profile');

  await makeRequest('GET', '/api/organization/1/departments', authHeader, null, step++, 'Get Organization Departments');
  await makeRequest('GET', '/api/organization/1/designations', authHeader, null, step++, 'Get Organization Designations');
  await makeRequest('GET', '/api/organization/1/locations', authHeader, null, step++, 'Get Organization Locations');

  // 3. Academic Year
  const ayRes = await makeRequest('POST', '/api/academicyear', authHeader, {
    organizationId: 1,
    startDate: '2026-01-01',
    endDate: '2026-12-31',
    isActive: true
  }, step++, 'Create Academic Year 2026');
  const academicYearId = ayRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/academicyear?organizationId=1', authHeader, null, step++, 'List All Academic Years');
  await makeRequest('GET', `/api/academicyear/${academicYearId}`, authHeader, null, step++, 'Get Academic Year by ID');
  await makeRequest('PUT', `/api/academicyear/${academicYearId}`, authHeader, {
    startDate: '2026-01-01',
    endDate: '2026-12-31',
    isActive: true
  }, step++, 'Update Academic Year');

  // 4. Campus Location & Geofencing
  const locRes = await makeRequest('POST', '/api/location', authHeader, {
    organizationId: 1,
    name: 'Bengaluru Innovation Hub',
    address: 'Outer Ring Road, Bellandur',
    city: 'Bengaluru',
    state: 'Karnataka',
    country: 'India',
    postalCode: '560103',
    emailAlias: 'blr-campus@hrm.local',
    contactNumber: '+91-80-44990011',
    latitude: 12.927900,
    longitude: 77.683300,
    radius: 500,
    timeZone: 'IST',
    timeZoneValue: 'India Standard Time'
  }, step++, 'Create Campus Location with GPS & Geofence Radius');
  const locationId = locRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/location?organizationId=1', authHeader, null, step++, 'List Campus Locations');
  await makeRequest('GET', `/api/location/${locationId}`, authHeader, null, step++, 'Get Location by ID');
  await makeRequest('PUT', `/api/location/${locationId}`, authHeader, {
    name: 'Bengaluru Innovation Hub',
    address: 'Outer Ring Road, Bellandur',
    city: 'Bengaluru',
    state: 'Karnataka',
    country: 'India',
    postalCode: '560103',
    latitude: 12.927900,
    longitude: 77.683300,
    radius: 750,
    timeZoneValue: 'India Standard Time'
  }, step++, 'Update Location Geofence Radius to 750m');

  // 5. Departments
  const deptRes = await makeRequest('POST', '/api/department', authHeader, {
    organizationId: 1,
    name: 'Product Engineering',
    code: 'ENG-PROD',
    departmentHeadId: 1
  }, step++, 'Create Department');
  const departmentId = deptRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/department?organizationId=1', authHeader, null, step++, 'List Departments');
  await makeRequest('GET', `/api/department/${departmentId}`, authHeader, null, step++, 'Get Department by ID');
  await makeRequest('PUT', `/api/department/${departmentId}`, authHeader, {
    name: 'Product Architecture & Dev',
    code: 'ENG-ARCH',
    departmentHeadId: 1
  }, step++, 'Update Department');

  // 6. Designations
  const desigRes = await makeRequest('POST', '/api/designation', authHeader, {
    organizationId: 1,
    name: 'Principal Software Engineer',
    description: 'L5 Technical Lead',
    level: 5
  }, step++, 'Create Designation');
  const designationId = desigRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/designation?organizationId=1', authHeader, null, step++, 'List Designations');
  await makeRequest('GET', `/api/designation/${designationId}`, authHeader, null, step++, 'Get Designation by ID');
  await makeRequest('PUT', `/api/designation/${designationId}`, authHeader, {
    name: 'Staff Software Architect',
    description: 'L6 Architecture Lead',
    level: 6
  }, step++, 'Update Designation');

  // 7. Roles & Permissions
  await makeRequest('GET', '/api/role/permissions?organizationId=1', authHeader, null, step++, 'Get Master Permissions Catalog');

  const roleRes = await makeRequest('POST', '/api/role', authHeader, {
    organizationId: 1,
    name: 'Technical Lead',
    description: 'Team lead with shift & OT view rights',
    isActive: true,
    permissionIds: []
  }, step++, 'Create Custom Role');
  const roleId = roleRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/role?organizationId=1', authHeader, null, step++, 'List Roles');
  await makeRequest('GET', `/api/role/${roleId}`, authHeader, null, step++, 'Get Role by ID');
  await makeRequest('PUT', `/api/role/${roleId}`, authHeader, {
    name: 'Senior Technical Lead',
    description: 'Updated lead privileges',
    isActive: true
  }, step++, 'Update Role');
  await makeRequest('POST', '/api/role/permissions/assign', authHeader, {
    roleId,
    permissionIds: [1, 2]
  }, step++, 'Assign Permissions to Role');

  // 8. Shifts
  const shiftRes = await makeRequest('POST', '/api/shift', authHeader, {
    organizationId: 1,
    locationId,
    name: 'Standard Morning Shift',
    inTime: '09:00:00',
    outTime: '18:00:00',
    isOvernight: false,
    graceMinutes: 15,
    breakMinutes: 60,
    breakStartTime: '13:00:00',
    breakEndTime: '14:00:00'
  }, step++, 'Create Shift with Break Times & Grace Window');
  const shiftId = shiftRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/shift?organizationId=1', authHeader, null, step++, 'List Shifts');
  await makeRequest('GET', `/api/shift/${shiftId}`, authHeader, null, step++, 'Get Shift by ID');
  await makeRequest('PUT', `/api/shift/${shiftId}`, authHeader, {
    locationId,
    name: 'Standard Morning Shift',
    inTime: '09:00:00',
    outTime: '18:00:00',
    isOvernight: false,
    graceMinutes: 20,
    breakMinutes: 60,
    breakStartTime: '13:00:00',
    breakEndTime: '14:00:00'
  }, step++, 'Update Shift Grace Minutes to 20');

  // 9. Off-Day Matrix
  const offdayRes = await makeRequest('POST', '/api/offday', authHeader, {
    organizationId: 1,
    academicYearId,
    locationId,
    roleId: 4,
    offDayName: 'Sunday',
    workDayType: 1,
    week1: true, week2: true, week3: true, week4: true, week5: true, week6: true
  }, step++, 'Create Weekly Off-Day Rule (Sunday Off)');
  const offdayId = offdayRes.data?.data?.id || 1;

  await makeRequest('GET', `/api/offday?organizationId=1&academicYearId=${academicYearId}`, authHeader, null, step++, 'List Off-Days');
  await makeRequest('GET', `/api/offday/${offdayId}`, authHeader, null, step++, 'Get Off-Day by ID');
  await makeRequest('PUT', `/api/offday/${offdayId}`, authHeader, {
    academicYearId,
    locationId,
    roleId: 4,
    offDayName: 'Sunday (Full Day)',
    workDayType: 1,
    week1: true, week2: true, week3: true, week4: true, week5: true, week6: true
  }, step++, 'Update Off-Day Rule');

  // 10. Public Holidays
  const holidayRes = await makeRequest('POST', '/api/holiday', authHeader, {
    organizationId: 1,
    academicYearId,
    locationId,
    name: 'Independence Day',
    date: '2026-08-15',
    isOptional: false
  }, step++, 'Create Public Holiday');
  const holidayId = holidayRes.data?.data?.id || 1;

  await makeRequest('GET', `/api/holiday?organizationId=1&academicYearId=${academicYearId}`, authHeader, null, step++, 'List Public Holidays');
  await makeRequest('GET', `/api/holiday/${holidayId}`, authHeader, null, step++, 'Get Holiday by ID');
  await makeRequest('PUT', `/api/holiday/${holidayId}`, authHeader, {
    locationId,
    name: 'National Independence Day',
    date: '2026-08-15',
    isOptional: false
  }, step++, 'Update Holiday');

  // 11. Leave Types
  const ltRes = await makeRequest('POST', '/api/leavetype', authHeader, {
    organizationId: 1,
    name: 'Annual Privilege Leave',
    description: 'Standard earned annual vacation',
    isActive: true,
    isPaid: true,
    leaves: 18,
    canTakeHalfDay: true,
    carryForwardLeaveCount: 6
  }, step++, 'Create Leave Type & Policy Settings');
  const leaveTypeId = ltRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/leavetype?organizationId=1', authHeader, null, step++, 'List Leave Types');
  await makeRequest('GET', `/api/leavetype/${leaveTypeId}`, authHeader, null, step++, 'Get Leave Type by ID');
  await makeRequest('PUT', `/api/leavetype/${leaveTypeId}`, authHeader, {
    name: 'Annual Privilege Leave',
    description: 'Standard earned annual vacation',
    isActive: true,
    isPaid: true,
    leaves: 20,
    canTakeHalfDay: true,
    carryForwardLeaveCount: 6
  }, step++, 'Update Leave Type Quota to 20');

  // 12. Overtime Policy
  const otRes = await makeRequest('POST', '/api/overtime/settings', authHeader, {
    organizationId: 1,
    name: 'Standard Hourly Overtime Policy',
    isOverTimeEnabled: true,
    otStartAfterMinutes: 30,
    multiplier: 1.50,
    maxOTHoursPerDay: 4.00,
    isActive: true
  }, step++, 'Create Overtime Policy Settings');
  const otSettingId = otRes.data?.data?.id || 1;

  await makeRequest('GET', '/api/overtime/settings?organizationId=1', authHeader, null, step++, 'Get Active Overtime Settings');
  await makeRequest('PUT', `/api/overtime/settings/${otSettingId}`, authHeader, {
    name: 'Standard Hourly Overtime Policy',
    isOverTimeEnabled: true,
    otStartAfterMinutes: 20,
    multiplier: 1.50,
    maxOTHoursPerDay: 4.00,
    isActive: true
  }, step++, 'Update Overtime Threshold to 20 mins');

  // 13. Employee Onboarding
  const empCode = `EMP${String(Date.now() % 100000).padStart(5, '0')}`;
  const empPayload = {
    organizationId: 1,
    employeeCode: empCode,
    firstName: 'Arjun',
    middleName: 'K',
    lastName: 'Verma',
    dob: '1995-05-20',
    gender: 'Male',
    bloodGroup: 'B+',
    maritalStatus: 'Single',
    fatherName: 'Kailash Verma',
    motherName: 'Sunita Verma',
    nationality: 'Indian',
    religion: 'Hindu',
    birthPlace: 'Delhi',
    employeeType: 'Full-Time',
    qualification: 'B.Tech Computer Science',
    skillSet: 'C#, .NET 10, React, Azure SQL',
    password: 'Password@123',
    workEmail: `arjun.${empCode.toLowerCase()}@hrm.local`,
    mobile: '+91-9988776655',
    address: 'Flat 402, Green Meadows',
    city: 'Bengaluru',
    state: 'Karnataka',
    country: 'India',
    postalCode: '560103',
    emergencyPerson: 'Kailash Verma',
    emergencyContact: '+91-9988776600',
    departmentId,
    designationId,
    locationId,
    shiftId,
    dateOfJoining: '2026-01-15',
    probationPeriodMonths: 3,
    roleIds: [4]
  };
  const empRes = await makeRequest('POST', '/api/employee', authHeader, empPayload, step++, 'Onboard New Employee with Full Details');
  const employeeId = empRes.data?.data?.id || 2;

  await makeRequest('GET', `/api/employee/${employeeId}`, authHeader, null, step++, 'Get Employee Profile');
  await makeRequest('GET', '/api/employee?organizationId=1&pageNumber=1&pageSize=10', authHeader, null, step++, 'Get Paged Employee Directory');
  await makeRequest('PUT', `/api/employee/${employeeId}`, authHeader, {
    ...empPayload,
    mobile: '+91-9988776688'
  }, step++, 'Update Employee Contact Number');

  // 14. Assignments & Balances
  await makeRequest('POST', '/api/role/assign', authHeader, { employeeId, roleId: 4 }, step++, 'Assign Role to Employee');
  await makeRequest('POST', '/api/shift/assign', authHeader, { employeeId, shiftId }, step++, 'Assign Shift to Employee');
  await makeRequest('POST', '/api/shift/assign/bulk', authHeader, { employeeIds: [employeeId], shiftId }, step++, 'Bulk Assign Shift to Employees');

  await makeRequest('POST', '/api/leave/balances/adjust', authHeader, {
    employeeId,
    leaveTypeId,
    academicYearId,
    leaveCredited: 20,
    leaveBroughtForward: 0,
    leavesTaken: 0
  }, step++, 'Credit Initial Leave Quota (20 Days)');
  await makeRequest('GET', `/api/leave/balances?employeeId=${employeeId}&academicYearId=${academicYearId}`, authHeader, null, step++, 'Verify Credited Leave Balances');

  // 15. Daily Attendance (Geofencing & Checkout OT)
  await makeRequest('POST', '/api/attendance/check-in', authHeader, {
    employeeId,
    locationId,
    latitude: 13.500000, // Outside geofence
    longitude: 78.500000,
    macId: '00:1B:44:11:3A:B7',
    inNetworkSource: 1,
    inPlatform: 1,
    appVersion: '1.0.0'
  }, step++, 'Test Geofence Rejection: Punch Outside Campus Radius');

  await makeRequest('POST', '/api/attendance/check-in', authHeader, {
    employeeId,
    locationId,
    latitude: 12.927920, // Inside geofence
    longitude: 77.683310,
    macId: '00:1B:44:11:3A:B7',
    inNetworkSource: 1,
    inPlatform: 1,
    appVersion: '1.0.0'
  }, step++, 'Test Valid Check-In Inside Campus Radius');

  await makeRequest('GET', '/api/attendance/today', authHeader, null, step++, 'Get Today\'s Punch Status (Logged-in User)');
  await makeRequest('GET', `/api/attendance/today/${employeeId}`, authHeader, null, step++, `Get Today\'s Punch Status for Employee ${employeeId} (Admin/Manager)`);

  await makeRequest('POST', '/api/attendance/check-out', authHeader, {
    employeeId,
    latitude: 12.927920,
    longitude: 77.683310,
    macId: '00:1B:44:11:3A:B7',
    outNetworkSource: 1,
    outPlatform: 1,
    remark: 'Completed sprint backlog items'
  }, step++, 'Punch Check-Out (Computes Day Total & Logs Overtime)');

  await makeRequest('GET', `/api/attendance/history?employeeId=${employeeId}`, authHeader, null, step++, 'Get 30-Day Attendance History with Overtime Remarks');

  // 15.1 Employee Dashboard & Unified Calendar
  await makeRequest('GET', '/api/dashboard/employee', authHeader, null, step++, 'Get Logged-in Employee Dashboard Summary');
  await makeRequest('GET', `/api/dashboard/employee/${employeeId}`, authHeader, null, step++, `Get Employee ${employeeId} Dashboard Summary (Admin)`);
  await makeRequest('GET', '/api/dashboard/employee/calendar', authHeader, null, step++, 'Get Employee Current Month Unified Calendar');
  await makeRequest('GET', `/api/dashboard/employee/${employeeId}/calendar?year=2026&month=9`, authHeader, null, step++, `Get Employee ${employeeId} September 2026 Unified Calendar Matrix`);

  // 16. Overtime Workflow
  await makeRequest('POST', '/api/overtime/record', authHeader, {
    employeeId,
    otSettingId,
    otDate: '2026-09-01',
    otHours: 2.50,
    hourlyRate: 150.00
  }, step++, 'Manually Record Overtime Entry');

  const pendingOtRes = await makeRequest('GET', '/api/overtime/pending?organizationId=1', authHeader, null, step++, 'List Pending Overtime Entries Requiring Approval');
  const otEntryId = pendingOtRes.data?.data?.[0]?.id || 1;

  await makeRequest('POST', `/api/overtime/${otEntryId}/approval`, authHeader, {
    isApproved: true,
    remarks: 'Overtime approved for project delivery'
  }, step++, 'Approve Overtime Entry');

  // 17. Leave Workflow (Overlap & Refund)
  const leaveRes = await makeRequest('POST', '/api/leave', authHeader, {
    employeeId,
    leaveTypeId,
    leaveFrom: '2026-10-12T00:00:00Z',
    leaveTo: '2026-10-14T00:00:00Z',
    noOfLeave: 3,
    isHalfDay: false,
    reason: 'Personal family event'
  }, step++, 'Apply for 3 Days of Leave');
  const leaveAppId = leaveRes.data?.data?.id || 1;

  await makeRequest('POST', '/api/leave', authHeader, {
    employeeId,
    leaveTypeId,
    leaveFrom: '2026-10-13T00:00:00Z',
    leaveTo: '2026-10-16T00:00:00Z',
    noOfLeave: 4,
    isHalfDay: false,
    reason: 'Overlapping request'
  }, step++, 'Test Leave Overlap Prevention: Should Return 400 Bad Request');

  await makeRequest('GET', `/api/leave/my-leaves/${employeeId}`, authHeader, null, step++, 'List Employee\'s Submitted Leave Requests');
  await makeRequest('GET', '/api/leave/pending?organizationId=1', authHeader, null, step++, 'List All Pending Leaves Needing Approval');

  await makeRequest('POST', `/api/leave/${leaveAppId}/approve`, authHeader, {
    isApproved: true,
    remarks: 'Approved by manager'
  }, step++, 'Approve Leave Application (Deducts Balance)');

  await makeRequest('GET', `/api/leave/balances?employeeId=${employeeId}&academicYearId=${academicYearId}`, authHeader, null, step++, 'Verify Deducted Leave Balance (LeavesTaken = 3)');

  await makeRequest('POST', `/api/leave/${leaveAppId}/cancel`, authHeader, null, step++, 'Cancel Approved Leave (Triggers Automatic Day Refund)');

  await makeRequest('GET', `/api/leave/balances?employeeId=${employeeId}&academicYearId=${academicYearId}`, authHeader, null, step++, 'Verify Restored Leave Balance (LeavesTaken Reverts to 0)');

  // 18. Notifications
  await makeRequest('GET', `/api/notification/${employeeId}`, authHeader, null, step++, 'Fetch Recent Employee Notifications');
  await makeRequest('PUT', '/api/notification/1/read', authHeader, null, step++, 'Mark Notification as Read');

  // 19. Teardown & Soft-Deletion
  await makeRequest('DELETE', `/api/shift/${shiftId}`, authHeader, null, step++, 'Soft-Delete Shift');
  await makeRequest('DELETE', `/api/offday/${offdayId}`, authHeader, null, step++, 'Soft-Delete Off-Day Rule');
  await makeRequest('DELETE', `/api/holiday/${holidayId}`, authHeader, null, step++, 'Soft-Delete Public Holiday');
  await makeRequest('DELETE', `/api/leavetype/${leaveTypeId}`, authHeader, null, step++, 'Soft-Delete Leave Type');
  await makeRequest('DELETE', `/api/department/${departmentId}`, authHeader, null, step++, 'Soft-Delete Department');
  await makeRequest('DELETE', `/api/designation/${designationId}`, authHeader, null, step++, 'Soft-Delete Designation');
  await makeRequest('DELETE', `/api/employee/${employeeId}`, authHeader, null, step++, 'Soft-Delete Employee');

  log('='.repeat(80));
  log(`Test Suite Completed. Total calls executed: ${RESULTS.length}`);
  log("Writing 'api_test_log.txt' and 'api_test_results.json'...");

  fs.writeFileSync('api_test_log.txt', LOG_LINES.join('\n'), 'utf-8');
  fs.writeFileSync('api_test_results.json', JSON.stringify(RESULTS, null, 2), 'utf-8');

  log("Saved successfully! Developers can open 'api_test_results.json' to see all exact request payloads and responses.");
  log('='.repeat(80));
}

runSuite().catch(console.error);
