#!/usr/bin/env python3
"""
HRAttendance - Comprehensive End-to-End API Test Suite
------------------------------------------------------
Tests all endpoints in exact dependency order and captures:
- Endpoint
- Method
- Request header
- Payload
- Response

Results are printed to console and saved to:
1. api_test_log.txt (formatted readable log)
2. api_test_results.json (structured JSON catalog for UI developers)
"""

import json
import urllib.request
import urllib.error
import sys
from datetime import datetime, timezone

BASE_URL = "http://localhost:5050"
RESULTS = []
LOG_LINES = []

def log(text=""):
    print(text)
    LOG_LINES.append(text)

def record_call(step_no, title, method, endpoint, headers, payload, status_code, response_body):
    record = {
        "step": step_no,
        "title": title,
        "method": method,
        "endpoint": endpoint,
        "request_headers": headers,
        "payload": payload,
        "status_code": status_code,
        "response": response_body
    }
    RESULTS.append(record)

    log("=" * 80)
    log(f"Step {step_no}: {title}")
    log(f"Endpoint: {endpoint}")
    log(f"Method: {method}")
    log(f"Request header : {json.dumps(headers, indent=2)}")
    if payload is not None:
        log(f"Payload:\n{json.dumps(payload, indent=2)}")
    else:
        log("Payload: None")
    
    if isinstance(response_body, (dict, list)):
        formatted_resp = json.dumps(response_body, indent=2)
    else:
        formatted_resp = str(response_body)
    log(f"Response: (HTTP {status_code})\n{formatted_resp}")
    log("=" * 80)
    log()

def make_request(method, path, headers=None, payload=None, step_no=1, title=""):
    url = f"{BASE_URL}{path}"
    req_headers = {"Content-Type": "application/json", "Accept": "application/json"}
    if headers:
        req_headers.update(headers)

    data_bytes = None
    if payload is not None:
        data_bytes = json.dumps(payload).encode("utf-8")

    req = urllib.request.Request(url, data=data_bytes, headers=req_headers, method=method)
    
    # Hide raw Bearer token in display header for readability, but keep format
    display_headers = dict(req_headers)
    if "Authorization" in display_headers and len(display_headers["Authorization"]) > 30:
        display_headers["Authorization"] = display_headers["Authorization"][:25] + "... [JWT TOKEN]"

    status_code = 0
    resp_data = None

    try:
        with urllib.request.urlopen(req) as resp:
            status_code = resp.status
            body_text = resp.read().decode("utf-8")
            try:
                resp_data = json.loads(body_text) if body_text else {}
            except Exception:
                resp_data = body_text
    except urllib.error.HTTPError as e:
        status_code = e.code
        body_text = e.read().decode("utf-8")
        try:
            resp_data = json.loads(body_text) if body_text else {}
        except Exception:
            resp_data = body_text
    except Exception as ex:
        status_code = 500
        resp_data = {"error": str(ex)}

    record_call(step_no, title, method, url, display_headers, payload, status_code, resp_data)
    return status_code, resp_data

def run_suite():
    log("================================================================================")
    log(f"Starting HRAttendance API Test Suite at {datetime.now(timezone.utc).isoformat()}")
    log(f"Target API Server: {BASE_URL}")
    log("================================================================================\n")

    step = 1

    # --------------------------------------------------------------------------
    # PHASE 1: AUTHENTICATION & TENANT SETUP
    # --------------------------------------------------------------------------
    # 1. Login as Admin
    login_payload = {
        "organizationId": 1,
        "employeeCodeOrEmail": "admin@hrm.local",
        "password": "Password@123"
    }
    status, res = make_request("POST", "/api/auth/login", None, login_payload, step, "Login as Admin to acquire JWT Bearer Token")
    step += 1

    token = ""
    if status == 200 and isinstance(res, dict) and res.get("data", {}).get("token"):
        token = res["data"]["token"]
    
    auth_header = {"Authorization": f"Bearer {token}"} if token else {}

    # 2. Get Organization
    make_request("GET", "/api/organization/1", auth_header, None, step, "Get Organization Details")
    step += 1

    # 3. Update Organization
    org_update_payload = {
        "name": "Acme Global HR Solutions",
        "phone": "+1-800-555-0199",
        "email": "corporate@acmeglobal.com",
        "website": "https://www.acmeglobal.com",
        "address": "100 Enterprise Boulevard, Suite 500",
        "city": "Austin",
        "state": "Texas",
        "country": "USA",
        "postalCode": "78701",
        "industry": "Information Technology",
        "taxId": "US-TAX-88992211",
        "logoPath": "https://assets.acmeglobal.com/logo.png",
        "currency": "USD",
        "fiscalYearStartMonth": 1
    }
    make_request("PUT", "/api/organization/1", auth_header, org_update_payload, step, "Update Organization Profile")
    step += 1

    # 4. Get Org Departments, Designations, Locations
    make_request("GET", "/api/organization/1/departments", auth_header, None, step, "Get Organization Departments")
    step += 1
    make_request("GET", "/api/organization/1/designations", auth_header, None, step, "Get Organization Designations")
    step += 1
    make_request("GET", "/api/organization/1/locations", auth_header, None, step, "Get Organization Locations")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 2: MASTER CORE DATA SETUP
    # --------------------------------------------------------------------------
    # 5. Create Academic Year
    ay_payload = {
        "organizationId": 1,
        "startDate": "2026-01-01",
        "endDate": "2026-12-31",
        "isActive": True
    }
    status, res = make_request("POST", "/api/academicyear", auth_header, ay_payload, step, "Create Academic Year 2026")
    step += 1
    academic_year_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/academicyear?organizationId=1", auth_header, None, step, "List All Academic Years")
    step += 1
    make_request("GET", f"/api/academicyear/{academic_year_id}", auth_header, None, step, "Get Academic Year by ID")
    step += 1
    make_request("PUT", f"/api/academicyear/{academic_year_id}", auth_header, {"startDate": "2026-01-01", "endDate": "2026-12-31", "isActive": True}, step, "Update Academic Year")
    step += 1

    # 6. Create Location with Geofencing
    loc_payload = {
        "organizationId": 1,
        "name": "Bengaluru Innovation Hub",
        "address": "Outer Ring Road, Bellandur",
        "city": "Bengaluru",
        "state": "Karnataka",
        "country": "India",
        "postalCode": "560103",
        "emailAlias": "blr-campus@hrm.local",
        "contactNumber": "+91-80-44990011",
        "latitude": 12.927900,
        "longitude": 77.683300,
        "radius": 500,
        "timeZone": "IST",
        "timeZoneValue": "India Standard Time"
    }
    status, res = make_request("POST", "/api/location", auth_header, loc_payload, step, "Create Campus Location with GPS & Geofence Radius")
    step += 1
    location_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/location?organizationId=1", auth_header, None, step, "List Campus Locations")
    step += 1
    make_request("GET", f"/api/location/{location_id}", auth_header, None, step, "Get Location by ID")
    step += 1
    make_request("PUT", f"/api/location/{location_id}", auth_header, {**loc_payload, "radius": 750}, step, "Update Location Geofence Radius to 750m")
    step += 1

    # 7. Create Department
    dept_payload = {"organizationId": 1, "name": "Product Development", "code": "PROD-DEV", "departmentHeadId": 1}
    status, res = make_request("POST", "/api/department", auth_header, dept_payload, step, "Create Department")
    step += 1
    department_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/department?organizationId=1", auth_header, None, step, "List Departments")
    step += 1
    make_request("GET", f"/api/department/{department_id}", auth_header, None, step, "Get Department by ID")
    step += 1
    make_request("PUT", f"/api/department/{department_id}", auth_header, {"name": "Product Engineering", "code": "ENG-PROD", "departmentHeadId": 1}, step, "Update Department")
    step += 1

    # 8. Create Designation
    desig_payload = {"organizationId": 1, "name": "Principal Software Engineer", "description": "L5 Technical Lead", "level": 5}
    status, res = make_request("POST", "/api/designation", auth_header, desig_payload, step, "Create Designation")
    step += 1
    designation_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/designation?organizationId=1", auth_header, None, step, "List Designations")
    step += 1
    make_request("GET", f"/api/designation/{designation_id}", auth_header, None, step, "Get Designation by ID")
    step += 1
    make_request("PUT", f"/api/designation/{designation_id}", auth_header, {"name": "Staff Software Architect", "description": "L6 Architecture Lead", "level": 6}, step, "Update Designation")
    step += 1

    # 9. Roles & Permissions Catalog
    make_request("GET", "/api/role/permissions?organizationId=1", auth_header, None, step, "Get Master Permissions Catalog")
    step += 1

    role_payload = {"organizationId": 1, "name": "Technical Lead", "description": "Team lead with shift & OT view rights", "isActive": True, "permissionIds": []}
    status, res = make_request("POST", "/api/role", auth_header, role_payload, step, "Create Custom Role")
    step += 1
    role_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/role?organizationId=1", auth_header, None, step, "List Roles")
    step += 1
    make_request("GET", f"/api/role/{role_id}", auth_header, None, step, "Get Role by ID")
    step += 1
    make_request("PUT", f"/api/role/{role_id}", auth_header, {"name": "Senior Technical Lead", "description": "Updated lead privileges", "isActive": True}, step, "Update Role")
    step += 1
    make_request("POST", "/api/role/permissions/assign", auth_header, {"roleId": role_id, "permissionIds": [1, 2]}, step, "Assign Permissions to Role")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 3: SCHEDULES, POLICIES & CALENDARS
    # --------------------------------------------------------------------------
    # 10. Shift Setup
    shift_payload = {
        "organizationId": 1,
        "locationId": location_id,
        "name": "Standard Morning Shift",
        "inTime": "09:00:00",
        "outTime": "18:00:00",
        "isOvernight": False,
        "graceMinutes": 15,
        "breakMinutes": 60,
        "breakStartTime": "13:00:00",
        "breakEndTime": "14:00:00"
    }
    status, res = make_request("POST", "/api/shift", auth_header, shift_payload, step, "Create Shift with Break Times & Grace Window")
    step += 1
    shift_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/shift?organizationId=1", auth_header, None, step, "List Shifts")
    step += 1
    make_request("GET", f"/api/shift/{shift_id}", auth_header, None, step, "Get Shift by ID")
    step += 1
    make_request("PUT", f"/api/shift/{shift_id}", auth_header, {**shift_payload, "graceMinutes": 20}, step, "Update Shift Grace Minutes to 20")
    step += 1

    # 11. Off-Day Matrix
    offday_payload = {
        "organizationId": 1,
        "academicYearId": academic_year_id,
        "locationId": location_id,
        "roleId": 4, # Standard Employee Role
        "offDayName": "Sunday",
        "workDayType": 1,
        "week1": True, "week2": True, "week3": True, "week4": True, "week5": True, "week6": True
    }
    status, res = make_request("POST", "/api/offday", auth_header, offday_payload, step, "Create Weekly Off-Day Rule (Sunday Off)")
    step += 1
    offday_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", f"/api/offday?organizationId=1&academicYearId={academic_year_id}", auth_header, None, step, "List Off-Days")
    step += 1
    make_request("GET", f"/api/offday/{offday_id}", auth_header, None, step, "Get Off-Day by ID")
    step += 1
    make_request("PUT", f"/api/offday/{offday_id}", auth_header, {**offday_payload, "offDayName": "Sunday (Full Day)"}, step, "Update Off-Day Rule")
    step += 1

    # 12. Public Holiday
    holiday_payload = {
        "organizationId": 1,
        "academicYearId": academic_year_id,
        "locationId": location_id,
        "name": "Independence Day",
        "date": "2026-08-15",
        "isOptional": False
    }
    status, res = make_request("POST", "/api/holiday", auth_header, holiday_payload, step, "Create Public Holiday")
    step += 1
    holiday_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", f"/api/holiday?organizationId=1&academicYearId={academic_year_id}", auth_header, None, step, "List Public Holidays")
    step += 1
    make_request("GET", f"/api/holiday/{holiday_id}", auth_header, None, step, "Get Holiday by ID")
    step += 1
    make_request("PUT", f"/api/holiday/{holiday_id}", auth_header, {**holiday_payload, "name": "National Independence Day"}, step, "Update Holiday")
    step += 1

    # 13. Leave Types & Policies
    leave_type_payload = {
        "organizationId": 1,
        "name": "Annual Privilege Leave",
        "description": "Standard earned annual vacation",
        "isActive": True,
        "isPaid": True,
        "leaves": 18,
        "canTakeHalfDay": True,
        "carryForwardLeaveCount": 6
    }
    status, res = make_request("POST", "/api/leavetype", auth_header, leave_type_payload, step, "Create Leave Type & Policy Settings")
    step += 1
    leave_type_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/leavetype?organizationId=1", auth_header, None, step, "List Leave Types")
    step += 1
    make_request("GET", f"/api/leavetype/{leave_type_id}", auth_header, None, step, "Get Leave Type by ID")
    step += 1
    make_request("PUT", f"/api/leavetype/{leave_type_id}", auth_header, {**leave_type_payload, "leaves": 20}, step, "Update Leave Type Quota to 20")
    step += 1

    # 14. Overtime Policy Settings
    ot_payload = {
        "organizationId": 1,
        "name": "Standard Hourly Overtime Policy",
        "isOverTimeEnabled": True,
        "otStartAfterMinutes": 30,
        "multiplier": 1.50,
        "maxOTHoursPerDay": 4.00,
        "isActive": True
    }
    status, res = make_request("POST", "/api/overtime/settings", auth_header, ot_payload, step, "Create Overtime Policy Settings")
    step += 1
    ot_setting_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    make_request("GET", "/api/overtime/settings?organizationId=1", auth_header, None, step, "Get Active Overtime Settings")
    step += 1
    make_request("PUT", f"/api/overtime/settings/{ot_setting_id}", auth_header, {**ot_payload, "otStartAfterMinutes": 20}, step, "Update Overtime Threshold to 20 mins")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 4: EMPLOYEE ONBOARDING & ASSIGNMENTS
    # --------------------------------------------------------------------------
    emp_code = f"EMP{int(datetime.now().timestamp()) % 100000:05d}"
    emp_payload = {
        "organizationId": 1,
        "employeeCode": emp_code,
        "firstName": "Arjun",
        "middleName": "K",
        "lastName": "Verma",
        "dob": "1995-05-20",
        "gender": "Male",
        "bloodGroup": "B+",
        "maritalStatus": "Single",
        "fatherName": "Kailash Verma",
        "motherName": "Sunita Verma",
        "nationality": "Indian",
        "religion": "Hindu",
        "birthPlace": "Delhi",
        "employeeType": "Full-Time",
        "qualification": "B.Tech Computer Science",
        "skillSet": "C#, .NET 10, React, Azure SQL",
        "password": "Password@123",
        "workEmail": f"arjun.{emp_code.lower()}@hrm.local",
        "mobile": "+91-9988776655",
        "address": "Flat 402, Green Meadows",
        "city": "Bengaluru",
        "state": "Karnataka",
        "country": "India",
        "postalCode": "560103",
        "emergencyPerson": "Kailash Verma",
        "emergencyContact": "+91-9988776600",
        "departmentId": department_id,
        "designationId": designation_id,
        "locationId": location_id,
        "shiftId": shift_id,
        "dateOfJoining": "2026-01-15",
        "probationPeriodMonths": 3,
        "roleIds": [4] # Employee Role
    }
    status, res = make_request("POST", "/api/employee", auth_header, emp_payload, step, "Onboard New Employee with Full Details")
    step += 1
    employee_id = res.get("data", {}).get("id", 2) if isinstance(res, dict) else 2

    make_request("GET", f"/api/employee/{employee_id}", auth_header, None, step, "Get Employee Profile")
    step += 1
    make_request("GET", "/api/employee?organizationId=1&pageNumber=1&pageSize=10", auth_header, None, step, "Get Paged Employee Directory")
    step += 1
    make_request("PUT", f"/api/employee/{employee_id}", auth_header, {**emp_payload, "mobile": "+91-9988776688"}, step, "Update Employee Contact Number")
    step += 1

    # Assignments
    make_request("POST", "/api/role/assign", auth_header, {"employeeId": employee_id, "roleId": 4}, step, "Assign Role to Employee")
    step += 1
    make_request("POST", "/api/shift/assign", auth_header, {"employeeId": employee_id, "shiftId": shift_id}, step, "Assign Shift to Employee")
    step += 1
    make_request("POST", "/api/shift/assign/bulk", auth_header, {"employeeIds": [employee_id], "shiftId": shift_id}, step, "Bulk Assign Shift to Employees")
    step += 1

    # Leave Balances
    balance_adjust_payload = {
        "employeeId": employee_id,
        "leaveTypeId": leave_type_id,
        "academicYearId": academic_year_id,
        "leaveCredited": 20,
        "leaveBroughtForward": 0,
        "leavesTaken": 0
    }
    make_request("POST", "/api/leave/balances/adjust", auth_header, balance_adjust_payload, step, "Credit Initial Leave Quota (20 Days)")
    step += 1
    make_request("GET", f"/api/leave/balances?employeeId={employee_id}&academicYearId={academic_year_id}", auth_header, None, step, "Verify Credited Leave Balances")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 5: ATTENDANCE OPERATIONS & BUSINESS RULES
    # --------------------------------------------------------------------------
    # 15. Check-In Outside Geofence (Expect 400 Bad Request)
    outside_geofence_payload = {
        "employeeId": employee_id,
        "locationId": location_id,
        "latitude": 13.500000, # Far away from 12.9279
        "longitude": 78.500000,
        "macId": "00:1B:44:11:3A:B7",
        "inNetworkSource": 1,
        "inPlatform": 1,
        "appVersion": "1.0.0"
    }
    make_request("POST", "/api/attendance/check-in", auth_header, outside_geofence_payload, step, "Test Geofence Rejection: Punch Outside Campus Radius")
    step += 1

    # 16. Check-In Inside Geofence (Expect 200 OK)
    inside_geofence_payload = {
        "employeeId": employee_id,
        "locationId": location_id,
        "latitude": 12.927920, # Inside 750m campus radius
        "longitude": 77.683310,
        "macId": "00:1B:44:11:3A:B7",
        "inNetworkSource": 1,
        "inPlatform": 1,
        "appVersion": "1.0.0"
    }
    make_request("POST", "/api/attendance/check-in", auth_header, inside_geofence_payload, step, "Test Valid Check-In Inside Campus Radius")
    step += 1

    make_request("GET", "/api/attendance/today", auth_header, None, step, "Get Today's Punch Status (Logged-in User)")
    step += 1
    make_request("GET", f"/api/attendance/today/{employee_id}", auth_header, None, step, f"Get Today's Punch Status for Employee {employee_id} (Admin/Manager)")
    step += 1

    # 17. Check-Out with Overtime Auto-Calculation
    checkout_payload = {
        "employeeId": employee_id,
        "latitude": 12.927920,
        "longitude": 77.683310,
        "macId": "00:1B:44:11:3A:B7",
        "outNetworkSource": 1,
        "outPlatform": 1,
        "remark": "Completed sprint backlog items"
    }
    make_request("POST", "/api/attendance/check-out", auth_header, checkout_payload, step, "Punch Check-Out (Computes Day Total & Logs Overtime)")
    step += 1

    make_request("GET", f"/api/attendance/history?employeeId={employee_id}", auth_header, None, step, "Get 30-Day Attendance History with Overtime Remarks")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 5.1: EMPLOYEE DASHBOARD & UNIFIED CALENDAR
    # --------------------------------------------------------------------------
    make_request("GET", "/api/dashboard/employee", auth_header, None, step, "Get Logged-in Employee Dashboard Summary")
    step += 1
    make_request("GET", f"/api/dashboard/employee/{employee_id}", auth_header, None, step, f"Get Employee {employee_id} Dashboard Summary (Admin)")
    step += 1
    make_request("GET", "/api/dashboard/employee/calendar", auth_header, None, step, "Get Employee Current Month Unified Calendar")
    step += 1
    make_request("GET", f"/api/dashboard/employee/{employee_id}/calendar?year=2026&month=9", auth_header, None, step, f"Get Employee {employee_id} September 2026 Unified Calendar Matrix")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 6: OVERTIME APPROVAL WORKFLOW
    # --------------------------------------------------------------------------
    manual_ot_payload = {
        "employeeId": employee_id,
        "otSettingId": ot_setting_id,
        "otDate": "2026-09-01",
        "otHours": 2.50,
        "hourlyRate": 150.00
    }
    make_request("POST", "/api/overtime/record", auth_header, manual_ot_payload, step, "Manually Record Overtime Entry")
    step += 1

    status, res = make_request("GET", "/api/overtime/pending?organizationId=1", auth_header, None, step, "List Pending Overtime Entries Requiring Approval")
    step += 1
    ot_entry_id = 1
    if isinstance(res, dict) and res.get("data") and len(res["data"]) > 0:
        ot_entry_id = res["data"][0]["id"]

    make_request("POST", f"/api/overtime/{ot_entry_id}/approval", auth_header, {"isApproved": True, "remarks": "Overtime approved for project delivery"}, step, "Approve Overtime Entry")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 7: LEAVE WORKFLOW & RULES (Overlap & Refund)
    # --------------------------------------------------------------------------
    leave_app_payload = {
        "employeeId": employee_id,
        "leaveTypeId": leave_type_id,
        "leaveFrom": "2026-10-12T00:00:00Z",
        "leaveTo": "2026-10-14T00:00:00Z",
        "noOfLeave": 3,
        "isHalfDay": False,
        "reason": "Personal family event"
    }
    status, res = make_request("POST", "/api/leave", auth_header, leave_app_payload, step, "Apply for 3 Days of Leave")
    step += 1
    leave_app_id = res.get("data", {}).get("id", 1) if isinstance(res, dict) else 1

    # Test Overlap Rejection
    overlap_payload = {
        "employeeId": employee_id,
        "leaveTypeId": leave_type_id,
        "leaveFrom": "2026-10-13T00:00:00Z",
        "leaveTo": "2026-10-16T00:00:00Z",
        "noOfLeave": 4,
        "isHalfDay": False,
        "reason": "Overlapping request"
    }
    make_request("POST", "/api/leave", auth_header, overlap_payload, step, "Test Leave Overlap Prevention: Should Return 400 Bad Request")
    step += 1

    make_request("GET", f"/api/leave/my-leaves/{employee_id}", auth_header, None, step, "List Employee's Submitted Leave Requests")
    step += 1
    make_request("GET", "/api/leave/pending?organizationId=1", auth_header, None, step, "List All Pending Leaves Needing Approval")
    step += 1

    # Approve Leave
    make_request("POST", f"/api/leave/{leave_app_id}/approve", auth_header, {"isApproved": True, "remarks": "Approved by manager"}, step, "Approve Leave Application (Deducts Balance)")
    step += 1
    make_request("GET", f"/api/leave/balances?employeeId={employee_id}&academicYearId={academic_year_id}", auth_header, None, step, "Verify Deducted Leave Balance (LeavesTaken = 3)")
    step += 1

    # Cancel Leave (Auto-Refund)
    make_request("POST", f"/api/leave/{leave_app_id}/cancel", auth_header, None, step, "Cancel Approved Leave (Triggers Automatic Day Refund)")
    step += 1
    make_request("GET", f"/api/leave/balances?employeeId={employee_id}&academicYearId={academic_year_id}", auth_header, None, step, "Verify Restored Leave Balance (LeavesTaken Reverts to 0)")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 8: NOTIFICATIONS
    # --------------------------------------------------------------------------
    make_request("GET", f"/api/notification/{employee_id}", auth_header, None, step, "Fetch Recent Employee Notifications")
    step += 1
    make_request("PUT", "/api/notification/1/read", auth_header, None, step, "Mark Notification as Read")
    step += 1

    # --------------------------------------------------------------------------
    # PHASE 9: TEARDOWN & SOFT-DELETION VERIFICATION
    # --------------------------------------------------------------------------
    make_request("DELETE", f"/api/shift/{shift_id}", auth_header, None, step, "Soft-Delete Shift")
    step += 1
    make_request("DELETE", f"/api/offday/{offday_id}", auth_header, None, step, "Soft-Delete Off-Day Rule")
    step += 1
    make_request("DELETE", f"/api/holiday/{holiday_id}", auth_header, None, step, "Soft-Delete Public Holiday")
    step += 1
    make_request("DELETE", f"/api/leavetype/{leave_type_id}", auth_header, None, step, "Soft-Delete Leave Type")
    step += 1
    make_request("DELETE", f"/api/department/{department_id}", auth_header, None, step, "Soft-Delete Department")
    step += 1
    make_request("DELETE", f"/api/designation/{designation_id}", auth_header, None, step, "Soft-Delete Designation")
    step += 1
    make_request("DELETE", f"/api/employee/{employee_id}", auth_header, None, step, "Soft-Delete Employee")
    step += 1

    # Save to files
    log("================================================================================")
    log(f"Test Suite Completed. Total calls executed: {len(RESULTS)}")
    log("Writing 'api_test_log.txt' and 'api_test_results.json'...")

    with open("api_test_log.txt", "w", encoding="utf-8") as f:
        f.write("\n".join(LOG_LINES))

    with open("api_test_results.json", "w", encoding="utf-8") as f:
        json.dump(RESULTS, f, indent=2)

    log("Saved successfully! Developers can use 'api_test_results.json' for exact UI request/response schemas.")
    log("================================================================================")

if __name__ == "__main__":
    run_suite()
