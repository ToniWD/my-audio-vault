import React, { useCallback } from 'react';
import {
    View,
    Text,
    TouchableOpacity,
    ScrollView,
    KeyboardAvoidingView,
    Platform,
    StyleSheet,
} from 'react-native';
import Feather, { FeatherIconName } from '@react-native-vector-icons/feather';
import { Colors } from '@/constants/colors';
import { globalStyles } from '@/constants/globalStyles';
import { DeleteAccountRequest, ProblemDetails, UserDto } from '@/api/types.gen';
import { useAuth } from '@/hooks/useAuth';
import { useModal } from '@/components/modal/ModalContext';
import { deleteApiUsersMe } from '@/api';
import { handleApiError } from '@/utils/helperApiError';
import { RoleLabels } from '@/utils/roleLabels';
import { router } from 'expo-router';

export const ProfileCard: React.FC<{ userData: UserDto | null }> = ({ userData }) => {
    const fullName = userData ? `${userData.firstName} ${userData.lastName}`.trim() || userData.username : 'No user found';
    const roleName = userData ? RoleLabels[userData.role] ?? 'Unknown' : 'Unknown';

    return (
        <View style={[globalStyles.card, styles.cardSpacing]}>
            <View style={globalStyles.avatar}>
                <Feather name="user" size={40} color={Colors.mainTheme.primary} />
            </View>
            <View style={globalStyles.flex1}>
                <Text style={globalStyles.heading}>{fullName}</Text>
                <Text style={globalStyles.caption}>{roleName}</Text>
            </View>
        </View>
    );
};

interface MenuItem {
    id: string;
    label: string;
    icon: FeatherIconName;
    adminOnly?: boolean;
    isDanger?: boolean;
    onPress?: () => void;
}

export const ProfileScreen = () => {
    const { user, logout } = useAuth();

    const { showModal } = useModal();

    const deleteAccount = useCallback(async (data: DeleteAccountRequest) => {
        const result = await deleteApiUsersMe({
            body: data
        });
        if (result.error) {
            console.error(result.error);
            handleApiError(result.error as ProblemDetails, showModal);
        } else {
            logout();
        }
    }, [logout, showModal]);

    const handleDeleteAccount = () => {
        showModal({
            title: 'Delete Account',
            message: 'Are you sure you want to delete your account? This action cannot be undone.',
            acceptLabel: 'Delete',
            refuseLabel: 'Cancel',

            hasInput: true,
            inputPlaceholder: 'Password',
            secureTextEntry: true,
            onAccept: (password) => {
                if (password) deleteAccount({ password: password })
            },
            onRefuse: () => console.log('Action cancelled'),
        });
    };

    const handleLogout = () => {
        showModal({
            title: 'Logout',
            message: 'Are you sure you want to log out?',
            acceptLabel: 'Logout',
            refuseLabel: 'Cancel',
            onAccept: () => {
                console.log("Logout");
                logout();
            },
        });
    };

    const allMenuItems: MenuItem[] = [
        {
            id: 'edit_profile',
            label: 'Edit Profile',
            icon: 'edit-3',
            onPress: () => {
                router.push('/profile/edit')
            }
        },
        {
            id: 'change_password',
            label: 'Change Password',
            icon: 'lock',
            onPress: () => {
                router.push('/profile/password')
            }
        },
        {
            id: 'settings',
            label: 'Settings',
            icon: 'sliders',
            onPress: () => {
                console.log("Settings")
                // onNavigate?.('Settings')
            }
        },
        {
            id: 'adminDashboard',
            label: 'Admin Dashboard',
            icon: 'layout',
            adminOnly: true,
            onPress: () => {
                router.push('/(admin)/dashboard')
            }
        },
        {
            id: 'delete_account',
            label: 'Delete Account',
            icon: 'trash-2',
            isDanger: true,
            onPress: handleDeleteAccount
        },
        {
            id: 'logout',
            label: 'Logout',
            icon: 'log-out',
            isDanger: true,
            onPress: handleLogout,
        },
    ];

    const menuItems = allMenuItems.filter(item => {
        if (item.adminOnly) {
            return user?.role === 3;
        }
        return true;
    });

    return (
        <KeyboardAvoidingView
            style={globalStyles.screen}
            behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        >
            <ScrollView
                contentContainerStyle={styles.scrollContent}
                showsVerticalScrollIndicator={false}
            >
                {/* <Text style={globalStyles.title}>Profile</Text>
                <Text style={globalStyles.subtitle}>Manage your account</Text> */}

                <ProfileCard userData={user} />

                {/* Menu as a list */}
                <View style={globalStyles.cardList}>
                    {menuItems.map((item, index) => {
                        const isLast = index === menuItems.length - 1;

                        return (
                            <TouchableOpacity
                                key={item.id}
                                style={[
                                    globalStyles.listItem,
                                    !isLast && globalStyles.listItemDivider,
                                ]}
                                onPress={item.onPress}
                                activeOpacity={0.7}
                            >
                                <View style={globalStyles.rowCentered}>
                                    <Feather
                                        name={item.icon}
                                        size={20}
                                        color={item.isDanger ? Colors.mainTheme.error : Colors.mainTheme.textPrimary} />
                                    <Text style={item.isDanger ? globalStyles.listItemTextDanger : globalStyles.listItemText}>
                                        {item.label}
                                    </Text>
                                </View>
                                <Feather
                                    name="chevron-right"
                                    size={18}
                                    color={item.isDanger ? Colors.mainTheme.error : Colors.mainTheme.textSecondary}
                                />
                            </TouchableOpacity>
                        );
                    })}
                </View>
            </ScrollView>
        </KeyboardAvoidingView>
    );
};

const styles = StyleSheet.create({
    scrollContent: {
        padding: 20,
        // maxWidth: 600,
        width: '100%',
        alignSelf: 'center',
    },
    cardSpacing: {
        gap: 16,
        marginBottom: 24,
    },
});

export default ProfileScreen;
