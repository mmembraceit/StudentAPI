Method: POST
URL: http://localhost:5173/api/students
Body → raw → JSON

{
  "tenantId": "11111111-1111-1111-1111-111111111111",
  "name": "Miguel",
  "dateOfBirth": "2000-05-14"
}


Check Persistance:  

GET all: 
http://localhost:5173/api/students?tenantId=11111111-1111-1111-1111-111111111111

PUT:
http://localhost:5173/api/students/TU_ID?tenantId=11111111-1111-1111-1111-111111111111
Body JSON:
{
  "name": "Miguel Updated",
  "dateOfBirth": "2000-05-15"
}

DELETE:
http://localhost:5173/api/students/TU_ID?tenantId=11111111-1111-1111-1111-111111111111