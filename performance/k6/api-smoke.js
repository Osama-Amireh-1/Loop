import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = (__ENV.BASE_URL ?? 'http://localhost:5000').replace(/\/$/, '');

export const options = {
  vus: 10,
  duration: '30s',
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<750']
  }
};

export default function () {
  const response = http.get(`${BASE_URL}/health`);

  check(response, {
    'health status is 200': (r) => r.status === 200,
    'health response has body': (r) => r.body && r.body.length > 0
  });

  sleep(1);
}
