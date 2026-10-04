import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserPresenceComponent } from './user-presence.component';

describe('UserPresenceComponent', () => {
  let component: UserPresenceComponent;
  let fixture: ComponentFixture<UserPresenceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserPresenceComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserPresenceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return uppercase initials or fallback', () => {
    expect(component.getInitial(' priya ')).toBe('P');
    expect(component.getInitial('   ')).toBe('?');
  });

  it('should list remaining users after the first four', () => {
    expect(
      component.getRemainingUserNames([
        { userId: '1', userName: 'One' },
        { userId: '2', userName: 'Two' },
        { userId: '3', userName: 'Three' },
        { userId: '4', userName: 'Four' },
        { userId: '5', userName: 'Five' },
        { userId: '6', userName: 'Six' },
      ]),
    ).toBe('Five, Six');
  });
});
