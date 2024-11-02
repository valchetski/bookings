# Task description
Create  a program to manage hotel room availability and reservations.
The application should read from files containing hotel data and booking data,
then allow a user to check room availability for a specified hotel, date range, and room type.   


Example command to run the program:   
`myapp
 --hotels hotels.json
 --bookings bookings.json`

Example: hotels.json   
```json
[
  {
    "id": "H1",
    "name": "Hotel California",
    "roomTypes": [
      {
        "code": "SGL",
        "description": "Single Room",
        "amenities": [
          "WiFi",
          "TV"
        ],
        "features": [
          "Non-smoking"
        ]
      },
      {
        "code": "DBL",
        "description": "Double Room",
        "amenities": [
          "WiFi",
          "TV",
          "Minibar"
        ],
        "features": [
          "Non-smoking",
          "Sea View"
        ]
      }
    ],
    "rooms": [
      {
        "roomType": "SGL",
        "roomId": "101"
      },
      {
        "roomType": "SGL",
        "roomId": "102"
      },
      {
        "roomType": "DBL",
        "roomId": "201"
      },
      {
        "roomType": "DBL",
        "roomId": "202"
      }
    ]
  }
]
```


Example: bookings.json   
```json
[
  {
    "hotelId": "H1",
    "arrival": "20240901",
    "departure": "20240903",
    "roomType": "DBL",
    "roomRate": "Prepaid"
  },
  {
    "hotelId": "H1",
    "arrival": "20240902",
    "departure": "20240905",
    "roomType": "SGL",
    "roomRate": "Standard"
  }
]
```

The program should implement the 2 commands described below.  

The program should exit when a blank line is entered.   

## Availability Command  

Example console input:  

`Availability(H1, 20240901, SGL)`  
`Availability(H1, 20240901-20240903, DBL)`

Output: the program should give the availability count for the specified room type and date range. 

Note: hotels sometimes accept overbookings so the value can be negative to indicate this.  

## Search Command  

Example input:

`Search(H1, 365, SGL)`

The program should return a comma separated list of date ranges and availability where the room is available. The 365 is the number of days to look ahead.  

If there is no availability the program should return an empty line.  

Example output:  

`(20240901-20240903, 3), (20240903-20240901, 1)`

General notes: 

Try to code the challenge as you would approach any typical work task; 
we are not looking for you to show knowledge of frameworks or unusual programming language features.
Most importantly, keep it simple. Computational complexity is not going to be a priority when the solution is evaluated. 

For everything that has not been specified assume that we had a simple solution in mind.
When in doubt feel free to ask questions. 

# Prompts
Here is the list of prompts for GitHub Copilot I've used:
- [Prompt that has full description of the task from the email I've received].
- 