import { User } from "./user";

export class UserParams {
  gender: string;
  pageSize = 5;
  pageNumber = 1;
  minAge = 18;
  maxAge = 99;
  orderBy = 'lastActive';

  constructor(user: User) {
    this.gender = user.gender === "male"? "female": "male";
  }
}
