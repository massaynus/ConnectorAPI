import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  vus: 10000,
  duration: '30s',
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

  console.log(res.status)
  sleep(1);
}
