import fs from 'fs';
import crypto from 'crypto';

const rawResults = JSON.parse(fs.readFileSync('api_test_results.json', 'utf-8'));

// Categorize items into organized Postman folders
function getCategory(title, endpoint, method) {
  if (endpoint.includes('/api/auth') || endpoint.includes('/api/organization')) return '1. Auth & Organization';
  if (endpoint.includes('/api/academicyear') || endpoint.includes('/api/location') || endpoint.includes('/api/department') || endpoint.includes('/api/designation') || endpoint.includes('/api/role')) {
    if (method === 'DELETE') return '9. Teardown & Deletion';
    return '2. Master Core Setup';
  }
  if (endpoint.includes('/api/shift') || endpoint.includes('/api/offday') || endpoint.includes('/api/holiday') || endpoint.includes('/api/leavetype')) {
    if (method === 'DELETE') return '9. Teardown & Deletion';
    return '3. Schedules & Policies';
  }
  if (endpoint.includes('/api/employee')) {
    if (method === 'DELETE') return '9. Teardown & Deletion';
    return '4. Employee Management';
  }
  if (endpoint.includes('/api/attendance')) return '5. Attendance & Geofence';
  if (endpoint.includes('/api/overtime')) return '6. Overtime Workflow';
  if (endpoint.includes('/api/leave')) return '7. Leave Management';
  if (endpoint.includes('/api/notification')) return '8. Notifications';
  return 'General';
}

const foldersMap = new Map();

for (const call of rawResults) {
  const category = getCategory(call.title, call.endpoint, call.method);
  if (!foldersMap.has(category)) {
    foldersMap.set(category, []);
  }

  // Parse path from endpoint
  const urlObj = new URL(call.endpoint);
  const pathSegments = urlObj.pathname.split('/').filter(Boolean);
  const queryParams = [];
  urlObj.searchParams.forEach((value, key) => {
    queryParams.push({ key, value });
  });

  const headers = [
    { key: 'Content-Type', value: 'application/json' },
    { key: 'Accept', value: 'application/json' }
  ];

  const hasAuth = call.request_headers && call.request_headers.Authorization;
  if (hasAuth) {
    headers.push({
      key: 'Authorization',
      value: 'Bearer {{token}}',
      description: 'JWT Bearer token automatically acquired from Login'
    });
  }

  const postmanItem = {
    name: `Step ${call.step}: ${call.title}`,
    request: {
      method: call.method,
      header: headers,
      url: {
        raw: `{{baseUrl}}${urlObj.pathname}${urlObj.search}`,
        host: ['{{baseUrl}}'],
        path: pathSegments,
        query: queryParams.length > 0 ? queryParams : undefined
      }
    },
    response: [
      {
        name: `Recorded Response (HTTP ${call.status_code})`,
        originalRequest: {
          method: call.method,
          header: headers,
          url: {
            raw: `{{baseUrl}}${urlObj.pathname}${urlObj.search}`,
            host: ['{{baseUrl}}'],
            path: pathSegments
          }
        },
        status: call.status_code === 200 ? 'OK' : call.status_code === 201 ? 'Created' : call.status_code === 400 ? 'Bad Request' : 'Response',
        code: call.status_code,
        _postman_previewlanguage: 'json',
        header: [{ key: 'Content-Type', value: 'application/json' }],
        body: JSON.stringify(call.response, null, 2)
      }
    ]
  };

  // Add payload if present
  if (call.payload !== null && call.method !== 'GET' && call.method !== 'HEAD') {
    postmanItem.request.body = {
      mode: 'raw',
      raw: JSON.stringify(call.payload, null, 2),
      options: {
        raw: {
          language: 'json'
        }
      }
    };
    postmanItem.response[0].originalRequest.body = postmanItem.request.body;
  }

  // Add auto-token test script for Login endpoint
  if (call.endpoint.includes('/api/auth/login')) {
    postmanItem.event = [
      {
        listen: 'test',
        script: {
          type: 'text/javascript',
          exec: [
            'var jsonData = pm.response.json();',
            'if (jsonData && jsonData.data && jsonData.data.token) {',
            '    pm.environment.set("token", jsonData.data.token);',
            '    pm.collectionVariables.set("token", jsonData.data.token);',
            '    console.log("Bearer token automatically saved to {{token}} variable.");',
            '}'
          ]
        }
      }
    ];
  }

  foldersMap.get(category).push(postmanItem);
}

// Build final Collection v2.1.0
const collectionItems = [];
const sortedFolders = Array.from(foldersMap.keys()).sort();

for (const folderName of sortedFolders) {
  collectionItems.push({
    name: folderName,
    item: foldersMap.get(folderName)
  });
}

const postmanCollection = {
  info: {
    _postman_id: crypto.randomUUID(),
    name: 'HRAttendance API - Complete End-to-End Test Collection',
    description: 'Complete 82-endpoint test collection for HRAttendance generated from live end-to-end execution. Includes all request payloads, parameters, headers, and real recorded responses.',
    schema: 'https://schema.getpostman.com/json/collection/v2.1.0/collection.json'
  },
  item: collectionItems,
  variable: [
    {
      key: 'baseUrl',
      value: 'http://localhost:5050',
      type: 'string'
    },
    {
      key: 'token',
      value: '',
      type: 'string'
    }
  ]
};

fs.writeFileSync('HRAttendance.postman_collection.json', JSON.stringify(postmanCollection, null, 2), 'utf-8');
console.log('Successfully generated HRAttendance.postman_collection.json!');
