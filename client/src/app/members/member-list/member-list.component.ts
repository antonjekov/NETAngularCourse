import { Component, OnInit } from '@angular/core';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  members$: Observable<Member[]>;
  members: Member[];
  pagination: Pagination;
  userParams: UserParams;
  user: User;
  genderList= [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]

  constructor(private membersService: MembersService) {
    this.userParams = this.membersService.getUserParams();
   }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(){
    this.membersService.setUserParams(this.userParams);
    this.membersService.getMembers(this.userParams)
      .subscribe(res =>
        {
          this.pagination = res.pagination;
          this.members = res.result;
        }
      );
  };

  pageChanged(event: PageChangedEvent): void {
    this.userParams.pageNumber = event.page;
    this.loadMembers();
  }

  resetFilters(){
    this.userParams = this.membersService.resetUserParams();
    this.membersService.setUserParams(this.userParams);
    this.loadMembers();
  }
}
