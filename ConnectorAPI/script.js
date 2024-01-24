import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  stages: [
    { duration: '10s', target: 10 }, // warm up round
    { duration: '5m', target: 100 }, // soak the API in a continuous realistic load
    { duration: '2m', target: 2000 }, // fast ramp-up to a high point
    { duration: '1m', target: 0 }, // quick ramp-down to 0 users
  ],
};


export default function () {
  const res = http.post('http://localhost:5048/Accessor',
    JSON.stringify({
      "guestNode": "ms2",
      "ownerNode": "ms1",
      "resourceId": "c4b53812-4d32-4060-2f59-08dc189b5a8f",
      "resourceName": "Resource",
      "accessLevel": 6
    })
    , { headers: { 'Content-Type': 'application/json' } });

  sleep(1);
}
